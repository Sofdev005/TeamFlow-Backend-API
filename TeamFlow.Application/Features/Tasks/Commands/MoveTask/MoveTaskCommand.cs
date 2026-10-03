using MediatR;
using TeamFlow.Application.Features.Tasks.Common;

namespace TeamFlow.Application.Features.Tasks.Commands.MoveTask;

public record MoveTaskCommand(
    Guid TaskId,
    Guid TargetColumnId,
    double NewOrder) : IRequest<TaskResponse>;