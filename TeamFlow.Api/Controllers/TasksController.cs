using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using TeamFlow.Application.Features.Tasks.Commands.CreateTask;
using TeamFlow.Application.Features.Tasks.Commands.MoveTask;
using TeamFlow.Application.Features.Tasks.Commands.UpdateTask;
using TeamFlow.Application.Features.Tasks.Commands.DeleteTask;
using TeamFlow.Application.Features.Tasks.Queries.GetTasks;

namespace TeamFlow.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/tasks")]
public class TasksController : ControllerBase
{
    private readonly ISender _mediator;

    public TasksController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetTasks([FromQuery] Guid projectId, CancellationToken cancellationToken)
    {
        var tasks = await _mediator.Send(new GetTasksQuery(projectId), cancellationToken);
        return Ok(tasks);
    }

    [HttpPost]
    public async Task<IActionResult> CreateTask([FromBody] CreateTaskCommand command, CancellationToken cancellationToken)
    {
        var task = await _mediator.Send(command, cancellationToken);
        return Ok(task);
    }

    [HttpPatch("{taskId}/move")]
    public async Task<IActionResult> MoveTask(Guid taskId, [FromBody] MoveTaskCommand command, CancellationToken cancellationToken)
    {
        var commandWithId = command with { TaskId = taskId };
        await _mediator.Send(commandWithId, cancellationToken);
        return NoContent();
    }

    [HttpPut("{taskId}")]
    public async Task<IActionResult> UpdateTask(Guid taskId, [FromBody] UpdateTaskCommand command, CancellationToken cancellationToken)
    {
        var commandWithId = command with { Id = taskId };
        var updated = await _mediator.Send(commandWithId, cancellationToken);
        return Ok(updated);
    }

    [HttpDelete("{taskId}")]
    public async Task<IActionResult> DeleteTask(Guid taskId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteTaskCommand(taskId), cancellationToken);
        return NoContent();
    }
}