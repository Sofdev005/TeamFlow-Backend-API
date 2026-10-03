using MediatR;
using TeamFlow.Application.Features.Tasks.Common;
using TeamFlow.Domain.Enums;

namespace TeamFlow.Application.Features.Tasks.Commands.CreateTask;

public record CreateTaskCommand(
    Guid ProjectId,
    Guid BoardColumnId,
    string Title,
    string? Description,
    TaskPriority Priority = TaskPriority.Medium,
    Guid? AssigneeId = null,
    DateTime? DueDate = null,
    double Order = 0,
    Guid? ParentTaskId = null) : IRequest<TaskResponse>;