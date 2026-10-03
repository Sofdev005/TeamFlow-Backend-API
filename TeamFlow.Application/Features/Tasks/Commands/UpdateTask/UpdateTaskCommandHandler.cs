using MediatR;
using Microsoft.EntityFrameworkCore;
using TeamFlow.Application.Common.Interfaces;
using TeamFlow.Application.Features.Tasks.Common;

namespace TeamFlow.Application.Features.Tasks.Commands.UpdateTask;

public class UpdateTaskCommandHandler : IRequestHandler<UpdateTaskCommand, TaskResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateTaskCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<TaskResponse> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User authentication context is missing.");

        var task = await _context.Tasks
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (task is null)
        {
            throw new KeyNotFoundException($"Task with ID '{request.Id}' was not found.");
        }

        // Apply rich domain behaviors
        task.MoveToColumn(request.BoardColumnId, request.Order);

        if (request.AssigneeId.HasValue)
        {
            task.AssignTo(request.AssigneeId.Value, currentUserId);
        }

        if (request.DueDate.HasValue)
        {
            task.SetDueDate(request.DueDate.Value);
        }

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