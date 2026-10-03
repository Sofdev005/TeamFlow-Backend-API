using MediatR;
using Microsoft.EntityFrameworkCore;
using TeamFlow.Application.Common.Interfaces;
using TeamFlow.Application.Features.Tasks.Common;

namespace TeamFlow.Application.Features.Tasks.Queries.GetTasks;

public class GetTasksQueryHandler : IRequestHandler<GetTasksQuery, List<TaskResponse>>
{
    private readonly IApplicationDbContext _context;

    public GetTasksQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<TaskResponse>> Handle(GetTasksQuery request, CancellationToken cancellationToken)
    {
        return await _context.Tasks
            .AsNoTracking()
            .Where(t => t.ProjectId == request.ProjectId)
            .OrderBy(t => t.Order)
            .Select(task => new TaskResponse(
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
                task.UpdatedAt))
            .ToListAsync(cancellationToken);
    }
}