using Microsoft.AspNetCore.Mvc;
using NasNeuron.ClaimsApi.Models;
using NasNeuron.ClaimsApi.Services;

namespace NasNeuron.ClaimsApi.Controllers;

[ApiController]
[Route("api/members")]
public class MembersController : ControllerBase
{
    private readonly List<Member> _members = SeedData.Members();

    [HttpGet]
    public ActionResult<IEnumerable<Member>> GetAll() => Ok(_members);

    [HttpGet("{id}")]
    public ActionResult<Member> Get(string id)
    {
        var member = _members.FirstOrDefault(m => m.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        return member is null ? NotFound() : Ok(member);
    }
}
