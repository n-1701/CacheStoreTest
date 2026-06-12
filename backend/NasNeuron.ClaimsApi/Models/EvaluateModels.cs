namespace NasNeuron.ClaimsApi.Models;

/// <summary>The claim context the UI submits for evaluation.</summary>
public class EvaluateRequest
{
    public string MemberId { get; set; } = "";
    public string Gender { get; set; } = "";
    public int Age { get; set; }
    public string ClaimType { get; set; } = "";
    public string Country { get; set; } = "";
    public string TreatmentDate { get; set; } = "";
    public decimal Amount { get; set; }

    /// <summary>If true, the evaluated claim is appended to claims history.</summary>
    public bool Record { get; set; } = true;
}

/// <summary>The decision returned to the UI (mirrors the ZEN agent's result block).</summary>
public class EvaluationResult
{
    public string Decision { get; set; } = "";
    public string Reason { get; set; } = "";
    public string RuleCode { get; set; } = "";
}
