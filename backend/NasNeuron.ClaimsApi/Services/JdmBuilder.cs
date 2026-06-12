using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using NasNeuron.ClaimsApi.Models;

namespace NasNeuron.ClaimsApi.Services;

/// <summary>
/// Builds the <c>claim_validation.json</c> JDM (JSON Decision Model) that the GoRules
/// ZEN engine executes. The graph is: inputNode -> decisionTableNode -> outputNode.
///
/// The decision table uses hitPolicy "first" (top-to-bottom, first match wins) and has:
///   inputs:  gender, age, claimType, and a free-form "condition" column for date/country logic
///   outputs: decision, reason, ruleCode
///
/// Each <see cref="Rule"/> becomes one row. Cell expressions are ZEN Expression Language.
/// </summary>
public class JdmBuilder
{
    // Stable column ids referenced by every rule row.
    private const string ColGender = "c_gender";
    private const string ColAge = "c_age";
    private const string ColClaimType = "c_claimtype";
    private const string ColExpr = "c_condition";   // free-form (date + country) column, no bound field
    private const string OutDecision = "o_decision";
    private const string OutReason = "o_reason";
    private const string OutRuleCode = "o_rulecode";

    public string Build(IEnumerable<Rule> rules)
    {
        var inputs = new JsonArray
        {
            Column(ColGender, "Gender", "gender"),
            Column(ColAge, "Age", "age"),
            Column(ColClaimType, "Claim type", "claimType"),
            Column(ColExpr, "Condition", null) // unbound -> cell is a full boolean expression
        };

        var outputs = new JsonArray
        {
            Column(OutDecision, "Decision", "decision"),
            Column(OutReason, "Reason", "reason"),
            Column(OutRuleCode, "Rule code", "ruleCode")
        };

        var ruleRows = new JsonArray();
        foreach (var r in rules.Where(r => r.Enabled))
            ruleRows.Add(RuleRow(r));

        var table = new JsonObject
        {
            ["id"] = "node_table",
            ["type"] = "decisionTableNode",
            ["name"] = "Claim validation",
            ["position"] = new JsonObject { ["x"] = 360, ["y"] = 160 },
            ["content"] = new JsonObject
            {
                ["hitPolicy"] = "first",
                ["inputs"] = inputs,
                ["outputs"] = outputs,
                ["rules"] = ruleRows
            }
        };

        var graph = new JsonObject
        {
            ["contentType"] = "application/vnd.gorules.decision",
            ["nodes"] = new JsonArray
            {
                new JsonObject
                {
                    ["id"] = "node_input",
                    ["type"] = "inputNode",
                    ["name"] = "Request",
                    ["position"] = new JsonObject { ["x"] = 80, ["y"] = 160 }
                },
                table,
                new JsonObject
                {
                    ["id"] = "node_output",
                    ["type"] = "outputNode",
                    ["name"] = "Response",
                    ["position"] = new JsonObject { ["x"] = 760, ["y"] = 160 }
                }
            },
            ["edges"] = new JsonArray
            {
                Edge("edge_in", "node_input", "node_table"),
                Edge("edge_out", "node_table", "node_output")
            }
        };

        return graph.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
    }

    private static JsonObject Column(string id, string name, string? field)
    {
        var c = new JsonObject { ["id"] = id, ["name"] = name };
        if (field is not null) c["field"] = field;
        return c;
    }

    private static JsonObject Edge(string id, string source, string target) => new()
    {
        ["id"] = id,
        ["type"] = "edge",
        ["sourceId"] = source,
        ["targetId"] = target
    };

    private JsonObject RuleRow(Rule r)
    {
        var row = new JsonObject
        {
            ["_id"] = r.Code,
            ["_description"] = r.Condition,
            // Input cells. Empty string = "matches anything" in a first-hit table.
            [ColGender] = r.Gender is null ? "" : ZenString(r.Gender),
            [ColClaimType] = r.ClaimType is null ? "" : ZenString(r.ClaimType),
            [ColAge] = r.AgeTest ?? "",
            [ColExpr] = ConditionExpression(r),
            // Output cells are ZEN expressions, so string outputs must be quoted literals.
            [OutDecision] = ZenString(r.Decision),
            [OutReason] = ZenString(r.Reason),
            [OutRuleCode] = ZenString(r.Code)
        };
        return row;
    }

    /// <summary>Builds the free-form ZEN boolean expression for date/country rules.</summary>
    private static string ConditionExpression(Rule r)
    {
        if (r.TreatmentOlderThanOneYear)
            // 'today' is injected into the evaluation context by the backend.
            return "date(today) - date(treatmentDate) > duration(\"365d\")";

        if (r.Kind == "daterange" && r.DateFrom is not null && r.DateTo is not null)
            return $"date(treatmentDate) < date({ZenString(r.DateFrom)}) or " +
                   $"date(treatmentDate) > date({ZenString(r.DateTo)})";

        if (r.Kind == "countries")
        {
            var parts = new List<string>();
            if (r.Excluded is { Count: > 0 })
                parts.Add($"country in {ZenList(r.Excluded)}");
            if (r.Included is { Count: > 0 })
                parts.Add($"not (country in {ZenList(r.Included)})");
            return parts.Count == 0 ? "" : string.Join(" or ", parts);
        }

        return "";
    }

    /// <summary>Quote and escape a value as a ZEN string literal.</summary>
    private static string ZenString(string value)
    {
        var sb = new StringBuilder("\"");
        foreach (var ch in value)
        {
            if (ch is '"' or '\\') sb.Append('\\');
            sb.Append(ch);
        }
        sb.Append('"');
        return sb.ToString();
    }

    private static string ZenList(IEnumerable<string> values) =>
        "[" + string.Join(", ", values.Select(ZenString)) + "]";
}
