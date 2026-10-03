using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeamFlow.Application.Features.Organizations.Commands.InviteMember;
using TeamFlow.Application.Features.Organizations.Queries.GetMembers;

namespace TeamFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrganizationsController : ControllerBase
{
    private readonly ISender _mediator;

    public OrganizationsController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("members/invite")]
    public async Task<IActionResult> InviteMember([FromBody] InviteMemberCommand command)
    {
        var memberId = await _mediator.Send(command);
        return Ok(new { MemberId = memberId });
    }

    [HttpGet("members")]
    public async Task<IActionResult> GetMembers()
    {
        var members = await _mediator.Send(new GetMembersQuery());
        return Ok(members);
    }
}