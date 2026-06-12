using Microsoft.AspNetCore.Mvc;
using NasNeuron.ClaimsApi.Models;
using NasNeuron.ClaimsApi.Services;

namespace NasNeuron.ClaimsApi.Controllers;

[ApiController]
[Route("api/rules")]
public class RulesController : ControllerBase
{
    private readonly RuleStore _store;
    private readonly ILogger<RulesController> _logger;

    public RulesController(RuleStore store, ILogger<RulesController> logger)
    {
        _store = store;
        _logger = logger;
    }

    [HttpGet]
    public ActionResult<IEnumerable<Rule>> GetAll() => Ok(_store.All());

    [HttpGet("{code}")]
    public ActionResult<Rule> Get(string code)
    {
        var rule = _store.Get(code);
        return rule is null ? NotFound() : Ok(rule);
    }

    /// <summary>Returns the current JDM document for the UI preview pane.</summary>
    [HttpGet("jdm")]
    public ContentResult GetJdm() =>
        Content(_store.CurrentJdm(), "application/json");

    /// <summary>
    /// Save a rule. This rebuilds the JDM, repackages the zip, and uploads it to the
    /// bucket. The agent hot-reloads within ~5s. Returns the saved rule on success.
    /// </summary>
    [HttpPut("{code}")]
    public async Task<ActionResult<Rule>> Save(string code, [FromBody] Rule rule, CancellationToken ct)
    {
        if (!string.Equals(code, rule.Code, StringComparison.OrdinalIgnoreCase))
            return BadRequest("Route code and body code must match.");

        try
        {
            var saved = await _store.SaveAsync(rule, ct);
            return Ok(saved);
        }
        catch (InvalidOperationException ex)
        {
            // Missing credentials etc. — surface a clean message to the UI.
            _logger.LogError(ex, "Rule save failed.");
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Rule publish failed.");
            return StatusCode(StatusCodes.Status502BadGateway, new { error = "Failed to publish ruleset to storage." });
        }
    }
}
