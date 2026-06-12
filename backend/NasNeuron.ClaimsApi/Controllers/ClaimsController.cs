using Microsoft.AspNetCore.Mvc;
using NasNeuron.ClaimsApi.Models;
using NasNeuron.ClaimsApi.Services;

namespace NasNeuron.ClaimsApi.Controllers;

[ApiController]
[Route("api/claims")]
public class ClaimsController : ControllerBase
{
    private readonly ClaimStore _store;
    private readonly ZenAgentClient _zen;
    private readonly ILogger<ClaimsController> _logger;

    public ClaimsController(ClaimStore store, ZenAgentClient zen, ILogger<ClaimsController> logger)
    {
        _store = store;
        _zen = zen;
        _logger = logger;
    }

    [HttpGet]
    public ActionResult<IEnumerable<Claim>> GetAll() => Ok(_store.All());

    /// <summary>
    /// Evaluate a claim against the live ZEN ruleset. If Record is true, the result is
    /// appended to claims history and the stored claim (with its new ID) is returned.
    /// </summary>
    [HttpPost("evaluate")]
    public async Task<ActionResult> Evaluate([FromBody] EvaluateRequest req, CancellationToken ct)
    {
        EvaluationResult result;
        try
        {
            result = await _zen.EvaluateAsync(req, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ZEN evaluation failed.");
            return StatusCode(StatusCodes.Status502BadGateway, new { error = "The ZEN agent could not be reached." });
        }

        Claim? recorded = null;
        if (req.Record)
        {
            recorded = _store.Add(new Claim
            {
                MemberId = req.MemberId,
                Name = LookupName(req.MemberId),
                Type = req.ClaimType,
                Country = req.Country,
                Date = req.TreatmentDate,
                Decision = result.Decision,
                Rule = result.RuleCode,
                Reason = result.Reason
            });
        }

        return Ok(new { result, claim = recorded });
    }

    private static string LookupName(string memberId) =>
        SeedData.Members().FirstOrDefault(m => m.Id == memberId)?.Name ?? memberId;
}
