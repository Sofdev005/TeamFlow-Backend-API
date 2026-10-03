using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeamFlow.Application.Features.Projects.Commands.CreateProject;
using TeamFlow.Application.Features.Projects.Members;
using TeamFlow.Application.Features.Projects.Queries.GetProjects;
using TeamFlow.Domain.Enums;

namespace TeamFlow.Api.Controllers;

public record AddProjectMemberRequest(Guid UserId, OrgRole Role);

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProjectsController : ControllerBase
{
    private readonly ISender _mediator;

    public ProjectsController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateProject([FromBody] CreateProjectCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetProjects), new { id = result.Id }, result);
    }

    [HttpGet]
    public async Task<IActionResult> GetProjects()
    {
        var result = await _mediator.Send(new GetProjectsQuery());
        return Ok(result);
    }

    [HttpGet("{projectId:guid}/members")]
    public async Task<IActionResult> GetProjectMembers(Guid projectId, CancellationToken cancellationToken)
    {
        var members = await _mediator.Send(new GetProjectMembersQuery(projectId), cancellationToken);
        return Ok(members);
    }

    [HttpPost("{projectId:guid}/members")]
    public async Task<IActionResult> AddProjectMember(
        Guid projectId,
        [FromBody] AddProjectMemberRequest request,
        CancellationToken cancellationToken)
    {
        var memberId = await _mediator.Send(
            new AddProjectMemberCommand(projectId, request.UserId, request.Role), cancellationToken);
        return Ok(new { memberId });
    }

    [HttpPost("{projectId:guid}/presence")]
    public async Task<IActionResult> RecordProjectPresence(Guid projectId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new RecordProjectPresenceCommand(projectId), cancellationToken);
        return NoContent();
    }
}