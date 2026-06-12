namespace NasNeuron.ClaimsApi.Models;

public class Member
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Gender { get; set; } = "";
    public int Age { get; set; }
    public string Dob { get; set; } = "";
    public string Policy { get; set; } = "";
    public string Status { get; set; } = "";
    public string Plan { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Joined { get; set; } = "";
    public int Dependents { get; set; }
}

public class Claim
{
    public string Id { get; set; } = "";
    public string MemberId { get; set; } = "";
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public string Country { get; set; } = "";
    public string Date { get; set; } = "";
    public string Decision { get; set; } = "";
    public string Rule { get; set; } = "";
    public string Reason { get; set; } = "";
}
