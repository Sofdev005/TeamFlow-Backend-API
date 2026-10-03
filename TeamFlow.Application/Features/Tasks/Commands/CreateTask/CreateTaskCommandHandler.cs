using MediatR;
using TeamFlow.Application.Common.Interfaces;
using TeamFlow.Application.Features.Tasks.Common;
using TeamFlow.Domain.Entities;

namespace TeamFlow.Application.Features.Tasks.Commands.CreateTask;

public class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, TaskResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CreateTaskCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<TaskResponse> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User authentication context is missing.");

        var task = new TaskItem(
            id: Guid.NewGuid(),
            projectId: request.ProjectId,
            boardColumnId: request.BoardColumnId,
            title: request.Title,
            reporterId: currentUserId,
            priority: request.Priority,
            order: request.Order,
            parentTaskId: request.ParentTaskId
        );

        if (request.AssigneeId.HasValue)
        {
            task.AssignTo(request.AssigneeId.Value, currentUserId);
        }

        if (request.DueDate.HasValue)
        {
            task.SetDueDate(request.DueDate.Value);
        }

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync(cancellationToken);

        return new TaskResponse(
            task.Id,
            task.ProjectId,
            task.BoardColumnId,
            task.Title,
            task.Description,
            task.Priority.ToString(),
            task.AssigneeId,
            task.ReporterId,
            task.DueDate,
            task.Order,
            task.CreatedAt,
            task.UpdatedAt);
    }
}