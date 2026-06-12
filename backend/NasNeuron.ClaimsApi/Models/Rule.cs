namespace NasNeuron.ClaimsApi.Models;

/// <summary>
/// A single validation rule. Maps 1:1 to a row in the GoRules ZEN decision table.
/// "Kind" drives how <see cref="Services.JdmBuilder"/> turns it into ZEN cell expressions.
/// </summary>
public class Rule
{
    public string Code { get; set; } = "";

    /// <summary>simple | daterange | countries | catch</summary>
    public string Kind { get; set; } = "simple";

    /// <summary>Human-readable summary shown in the UI.</summary>
    public string Condition { get; set; } = "";

    /// <summary>rejected | warning | approved</summary>
    public string Decision { get; set; } = "rejected";

    public string Reason { get; set; } = "";

    public bool Enabled { get; set; } = true;

    // --- structured condition parts (only the ones relevant to Kind are used) ---

    /// <summary>Equality test on the gender field, e.g. "male". Null = no test.</summary>
    public string? Gender { get; set; }

    /// <summary>Equality test on the claimType field, e.g. "maternity". Null = no test.</summary>
    public string? ClaimType { get; set; }

    /// <summary>Unary comparison on the age field, e.g. "> 17". Null = no test.</summary>
    public string? AgeTest { get; set; }

    /// <summary>T01-style flag: treatment date older than one year.</summary>
    public bool TreatmentOlderThanOneYear { get; set; }

    // daterange
    public string? DateFrom { get; set; }
    public string? DateTo { get; set; }

    // countries
    public List<string>? Included { get; set; }
    public List<string>? Excluded { get; set; }
}
