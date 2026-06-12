using NasNeuron.ClaimsApi.Models;

namespace NasNeuron.ClaimsApi.Services;

/// <summary>Initial demo data for the POC. In a real system these come from a database.</summary>
public static class SeedData
{
    public static List<Rule> Rules() =>
    [
        new Rule
        {
            Code = "T01", Kind = "simple",
            Condition = "Treatment date older than 1 year",
            Decision = "rejected",
            Reason = "Treatment date exceeds the 1-year claim window",
            TreatmentOlderThanOneYear = true
        },
        new Rule
        {
            Code = "T02", Kind = "daterange",
            Condition = "Treatment date outside coverage window (2025-07-01 \u2192 2026-06-30)",
            Decision = "rejected",
            Reason = "Treatment date falls outside the active policy coverage period",
            DateFrom = "2025-07-01", DateTo = "2026-06-30"
        },
        new Rule
        {
            Code = "G01", Kind = "countries",
            Condition = "Treatment country is excluded, or not in the included list",
            Decision = "rejected",
            Reason = "Treatment country is outside the policy\u2019s covered countries",
            Included = ["UAE", "Saudi Arabia", "Qatar", "Bahrain", "Kuwait", "Oman"],
            Excluded = ["North Korea", "Syria"]
        },
        new Rule
        {
            Code = "M01", Kind = "simple",
            Condition = "gender = male AND claimType = maternity",
            Decision = "rejected",
            Reason = "Male members cannot claim maternity benefits",
            Gender = "male", ClaimType = "maternity"
        },
        new Rule
        {
            Code = "M02", Kind = "simple",
            Condition = "claimType = pediatric AND age > 17",
            Decision = "rejected",
            Reason = "Pediatric claims are limited to members aged 17 or under",
            ClaimType = "pediatric", AgeTest = "> 17"
        },
        new Rule
        {
            Code = "W01", Kind = "simple",
            Condition = "claimType = dental AND age > 65",
            Decision = "warning",
            Reason = "Dental claims for members over 65 require manual clinical review before approval",
            ClaimType = "dental", AgeTest = "> 65"
        },
        new Rule
        {
            Code = "PASS", Kind = "catch",
            Condition = "catch-all",
            Decision = "approved",
            Reason = "No rejection rule matched \u2014 claim approved"
        }
    ];

    public static List<Member> Members() =>
    [
        new() { Id = "MBR-10241", Name = "Layla Haddad", Gender = "Female", Age = 34, Dob = "1991-04-12", Policy = "POL-558231", Status = "Active", Plan = "Premium Family", Email = "l.haddad@example.ae", Phone = "+971 50 220 1144", Joined = "2019-06-01", Dependents = 2 },
        new() { Id = "MBR-10242", Name = "Omar Khalil", Gender = "Male", Age = 41, Dob = "1984-11-03", Policy = "POL-558232", Status = "Active", Plan = "Standard", Email = "o.khalil@example.ae", Phone = "+971 55 880 7712", Joined = "2017-02-18", Dependents = 3 },
        new() { Id = "MBR-10243", Name = "Sara Mansour", Gender = "Female", Age = 29, Dob = "1996-07-22", Policy = "POL-558233", Status = "Pending", Plan = "Premium", Email = "s.mansour@example.ae", Phone = "+971 52 451 0098", Joined = "2024-09-30", Dependents = 0 },
        new() { Id = "MBR-10244", Name = "Yusuf Rahman", Gender = "Male", Age = 7, Dob = "2018-01-09", Policy = "POL-558234", Status = "Active", Plan = "Family \u00b7 Dependent", Email = "guardian@example.ae", Phone = "+971 50 113 8890", Joined = "2018-03-10", Dependents = 0 },
        new() { Id = "MBR-10245", Name = "Fatima Noor", Gender = "Female", Age = 68, Dob = "1957-09-15", Policy = "POL-558235", Status = "Active", Plan = "Senior Care", Email = "f.noor@example.ae", Phone = "+971 56 700 2231", Joined = "2010-05-20", Dependents = 1 },
        new() { Id = "MBR-10246", Name = "Hassan Ali", Gender = "Male", Age = 52, Dob = "1973-03-28", Policy = "POL-558236", Status = "Inactive", Plan = "Standard", Email = "h.ali@example.ae", Phone = "+971 54 332 6678", Joined = "2015-08-12", Dependents = 2 },
        new() { Id = "MBR-10247", Name = "Maryam Saeed", Gender = "Female", Age = 31, Dob = "1994-12-01", Policy = "POL-558237", Status = "Active", Plan = "Premium Family", Email = "m.saeed@example.ae", Phone = "+971 50 909 4456", Joined = "2021-01-15", Dependents = 1 },
        new() { Id = "MBR-10248", Name = "Tariq Aziz", Gender = "Male", Age = 19, Dob = "2006-06-18", Policy = "POL-558238", Status = "Active", Plan = "Youth", Email = "t.aziz@example.ae", Phone = "+971 55 221 0099", Joined = "2023-11-02", Dependents = 0 }
    ];

    public static List<Claim> Claims() =>
    [
        new() { Id = "CLM-90012", MemberId = "MBR-10242", Name = "Omar Khalil", Type = "maternity", Country = "UAE", Date = "2026-06-08", Decision = "rejected", Rule = "M01", Reason = "Male members cannot claim maternity benefits" },
        new() { Id = "CLM-90011", MemberId = "MBR-10247", Name = "Maryam Saeed", Type = "maternity", Country = "UAE", Date = "2026-06-07", Decision = "approved", Rule = "PASS", Reason = "No rejection rule matched \u2014 claim approved" },
        new() { Id = "CLM-90010", MemberId = "MBR-10248", Name = "Tariq Aziz", Type = "pediatric", Country = "UAE", Date = "2026-06-06", Decision = "rejected", Rule = "M02", Reason = "Pediatric claims are limited to members aged 17 or under" },
        new() { Id = "CLM-90009", MemberId = "MBR-10245", Name = "Fatima Noor", Type = "dental", Country = "UAE", Date = "2026-06-05", Decision = "warning", Rule = "W01", Reason = "Dental claims for members over 65 require manual clinical review before approval" },
        new() { Id = "CLM-90008", MemberId = "MBR-10244", Name = "Yusuf Rahman", Type = "pediatric", Country = "UAE", Date = "2026-06-04", Decision = "approved", Rule = "PASS", Reason = "No rejection rule matched \u2014 claim approved" },
        new() { Id = "CLM-90007", MemberId = "MBR-10241", Name = "Layla Haddad", Type = "general", Country = "UAE", Date = "2026-05-30", Decision = "rejected", Rule = "T01", Reason = "Treatment date exceeds the 1-year claim window" },
        new() { Id = "CLM-90006", MemberId = "MBR-10246", Name = "Hassan Ali", Type = "general", Country = "UAE", Date = "2025-06-20", Decision = "rejected", Rule = "T02", Reason = "Treatment date falls outside the active policy coverage period" },
        new() { Id = "CLM-90005", MemberId = "MBR-10243", Name = "Sara Mansour", Type = "optical", Country = "India", Date = "2026-05-12", Decision = "rejected", Rule = "G01", Reason = "Treatment country is outside the policy\u2019s covered countries" }
    ];
}
