using NasNeuron.ClaimsApi.Models;

namespace NasNeuron.ClaimsApi.Services;

/// <summary>
/// Holds the live ruleset in memory and orchestrates publishing on save:
/// update the rule, rebuild the JDM, repackage the zip, and upload it to iDrive e2.
/// The ZEN agent polls the bucket every ~5s and hot-reloads on change.
/// </summary>
public class RuleStore
{
    private readonly List<Rule> _rules;
    private readonly JdmBuilder _jdm;
    private readonly ZipPackager _zip;
    private readonly S3Uploader _s3;
    private readonly ILogger<RuleStore> _logger;
    private readonly SemaphoreSlim _gate = new(1, 1);

    public RuleStore(JdmBuilder jdm, ZipPackager zip, S3Uploader s3, ILogger<RuleStore> logger)
    {
        _jdm = jdm;
        _zip = zip;
        _s3 = s3;
        _logger = logger;
        _rules = SeedData.Rules();
    }

    public IReadOnlyList<Rule> All() => _rules;

    public Rule? Get(string code) =>
        _rules.FirstOrDefault(r => r.Code.Equals(code, StringComparison.OrdinalIgnoreCase));

    /// <summary>Current JDM document, for the UI's preview pane.</summary>
    public string CurrentJdm() => _jdm.Build(_rules);

    /// <summary>
    /// Insert or update a rule, then publish the full ruleset to the bucket.
    /// PASS (catch-all) is always kept last so first-match ordering holds.
    /// </summary>
    public async Task<Rule> SaveAsync(Rule incoming, CancellationToken ct = default)
    {
        await _gate.WaitAsync(ct);
        try
        {
            var existing = Get(incoming.Code);
            if (existing is not null)
            {
                var idx = _rules.IndexOf(existing);
                _rules[idx] = incoming;
            }
            else
            {
                // New rule goes above the catch-all.
                var passIdx = _rules.FindIndex(r => r.Kind == "catch");
                if (passIdx < 0) _rules.Add(incoming);
                else _rules.Insert(passIdx, incoming);
            }

            await PublishAsync(ct);
            return incoming;
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <summary>Serialize -> package -> upload. This is the hot-reload trigger.</summary>
    public async Task PublishAsync(CancellationToken ct = default)
    {
        var json = _jdm.Build(_rules);
        var bytes = _zip.Build(json);
        await _s3.UploadAsync(bytes, ct);
        _logger.LogInformation("Published ruleset: {Count} rules, {Bytes}-byte bundle.", _rules.Count, bytes.Length);
    }
}
