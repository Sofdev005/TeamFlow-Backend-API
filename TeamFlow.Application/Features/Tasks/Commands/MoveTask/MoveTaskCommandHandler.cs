using MediatR;
using Microsoft.EntityFrameworkCore;
using TeamFlow.Application.Common.Interfaces;
using TeamFlow.Application.Features.Tasks.Common;

namespace TeamFlow.Application.Features.Tasks.Commands.MoveTask;

public class MoveTaskCommandHandler : IRequestHandler<MoveTaskCommand, TaskResponse>
{
    private readonly IApplicationDbContext _context;

    public MoveTaskCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TaskResponse> Handle(MoveTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _context.Tasks
            .FirstOrDefaultAsync(t => t.Id == request.TaskId, cancellationToken);

        if (task is null)
        {
            throw new KeyNotFoundException($"Task with ID '{request.TaskId}' was not found.");
        }

        // A task can only move to a column on its own project's board.
        var targetColumn = await _context.BoardColumns
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.TargetColumnId, cancellationToken);

        if (targetColumn is null)
        {
            throw new KeyNotFoundException($"Board column with ID '{request.TargetColumnId}' was not found.");
        }

        if (targetColumn.BoardId != task.ProjectId)
        {
            throw new InvalidOperationException("The target column does not belong to the task's project.");
        }

        var sourceColumnId = task.BoardColumnId;
        var sourceTasks = await _context.Tasks
            .Where(candidate => candidate.ProjectId == task.ProjectId
                && candidate.BoardColumnId == sourceColumnId
                && candidate.Id != task.Id)
            .OrderBy(candidate => candidate.Order)
            .ThenBy(candidate => candidate.CreatedAt)
            .ToListAsync(cancellationToken);

        if (sourceColumnId == request.TargetColumnId)
        {
            var sourceIndex = Math.Clamp((int)request.NewOrder, 0, sourceTasks.Count);
            sourceTasks.Insert(sourceIndex, task);
            for (var index = 0; index < sourceTasks.Count; index++)
            {
                sourceTasks[index].MoveToColumn(request.TargetColumnId, index);
            }
        }
        else
        {
            var targetTasks = await _context.Tasks
                .Where(candidate => candidate.ProjectId == task.ProjectId
                    && candidate.BoardColumnId == request.TargetColumnId)
                .OrderBy(candidate => candidate.Order)
                .ThenBy(candidate => candidate.CreatedAt)
                .ToListAsync(cancellationToken);

            var targetIndex = Math.Clamp((int)request.NewOrder, 0, targetTasks.Count);
            targetTasks.Insert(targetIndex, task);

            for (var index = 0; index < sourceTasks.Count; index++)
            {
                sourceTasks[index].MoveToColumn(sourceColumnId, index);
            }

            for (var index = 0; index < targetTasks.Count; index++)
            {
                targetTasks[index].MoveToColumn(request.TargetColumnId, index);
            }
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