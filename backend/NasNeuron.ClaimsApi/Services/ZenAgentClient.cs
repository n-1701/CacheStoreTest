using System.Text.Json;
using NasNeuron.ClaimsApi.Models;

namespace NasNeuron.ClaimsApi.Services;

/// <summary>
/// Calls the GoRules ZEN agent's evaluate endpoint. The access token is attached
/// server-side via the X-Access-Token header and is never exposed to the browser.
///
/// POST {base}/api/projects/claim_validation.zip/evaluate/claim_validation.json
/// </summary>
public class ZenAgentClient
{
    private readonly HttpClient _http;
    private readonly string _evaluatePath;
    private readonly string _accessToken;
    private readonly ILogger<ZenAgentClient> _logger;

    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    public ZenAgentClient(HttpClient http, IConfiguration config, ILogger<ZenAgentClient> logger)
    {
        _http = http;
        _logger = logger;
        var baseUrl = (config["Zen:AgentBaseUrl"] ?? "https://agent-latest-jl93.onrender.com").TrimEnd('/');
        _http.BaseAddress = new Uri(baseUrl);
        _evaluatePath = "/api/projects/claim_validation.zip/evaluate/claim_validation.json";
        _accessToken = config["Zen:AccessToken"] ?? "nnhs-poc-token";
    }

    public async Task<EvaluationResult> EvaluateAsync(EvaluateRequest req, CancellationToken ct = default)
    {
        // Build the context. 'today' is supplied so date rules are deterministic.
        var context = new Dictionary<string, object?>
        {
            ["gender"] = req.Gender,
            ["age"] = req.Age,
            ["claimType"] = req.ClaimType,
            ["country"] = req.Country,
            ["treatmentDate"] = req.TreatmentDate,
            ["amount"] = req.Amount,
            ["today"] = DateTime.UtcNow.ToString("yyyy-MM-dd")
        };

        using var message = new HttpRequestMessage(HttpMethod.Post, _evaluatePath)
        {
            Content = JsonContent.Create(new { context })
        };
        message.Headers.Add("X-Access-Token", _accessToken);

        using var response = await _http.SendAsync(message, ct);
        var body = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("ZEN evaluate failed ({Status}): {Body}", (int)response.StatusCode, body);
            throw new HttpRequestException($"ZEN agent returned {(int)response.StatusCode}.");
        }

        return ParseResult(body);
    }

    /// <summary>Extracts the { decision, reason, ruleCode } block from the agent response.</summary>
    private static EvaluationResult ParseResult(string body)
    {
        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        // Agent wraps the decision output in a "result" object.
        var result = root.TryGetProperty("result", out var r) ? r : root;

        return new EvaluationResult
        {
            Decision = GetString(result, "decision"),
            Reason = GetString(result, "reason"),
            RuleCode = GetString(result, "ruleCode")
        };
    }

    private static string GetString(JsonElement el, string name) =>
        el.ValueKind == JsonValueKind.Object && el.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.String
            ? v.GetString() ?? ""
            : "";
}
