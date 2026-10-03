using MediatR;
using TeamFlow.Application.Features.Tasks.Common;
using TeamFlow.Domain.Enums;

namespace TeamFlow.Application.Features.Tasks.Commands.UpdateTask;

public record UpdateTaskCommand(
    Guid Id,
    Guid BoardColumnId,
    double Order,
    Guid? AssigneeId = null,
    DateTime? DueDate = null) : IRequest<TaskResponse>;