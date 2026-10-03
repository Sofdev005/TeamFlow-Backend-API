using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeamFlow.Application.Features.Boards.Commands.CreateBoardColumn;
using TeamFlow.Application.Features.Boards.Commands.ReorderBoardColumns;
using TeamFlow.Application.Features.Boards.Queries.GetBoardColumns;

namespace TeamFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BoardsController : ControllerBase
{
    private readonly ISender _mediator;

    public BoardsController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("columns")]
    public async Task<IActionResult> CreateColumn([FromBody] CreateBoardColumnCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpGet("columns/{projectId:guid}")]
    public async Task<IActionResult> GetColumns(Guid projectId)
    {
        var result = await _mediator.Send(new GetBoardColumnsQuery(projectId));
        return Ok(result);
    }
    [HttpPut("columns/reorder")]
    public async Task<IActionResult> ReorderColumns([FromBody] ReorderBoardColumnsCommand command)
    {
        await _mediator.Send(command);
        return NoContent();
    }
}