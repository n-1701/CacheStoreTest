using NasNeuron.ClaimsApi.Models;

namespace NasNeuron.ClaimsApi.Services;

/// <summary>In-memory claims history. Newest first.</summary>
public class ClaimStore
{
    private readonly List<Claim> _claims = SeedData.Claims();
    private int _seq = 90013;
    private readonly object _lock = new();

    public IReadOnlyList<Claim> All()
    {
        lock (_lock) return _claims.ToList();
    }

    public Claim Add(Claim claim)
    {
        lock (_lock)
        {
            claim.Id = $"CLM-{_seq++}";
            _claims.Insert(0, claim);
            return claim;
        }
    }
}
