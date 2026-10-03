using MediatR;
using Microsoft.EntityFrameworkCore;
using TeamFlow.Application.Common.Interfaces;
using TeamFlow.Application.Features.Boards.Common;
using TeamFlow.Domain.Entities;

namespace TeamFlow.Application.Features.Boards.Queries.GetBoardColumns;

public class GetBoardColumnsQueryHandler : IRequestHandler<GetBoardColumnsQuery, List<BoardColumnResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetBoardColumnsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<List<BoardColumnResponse>> Handle(GetBoardColumnsQuery request, CancellationToken cancellationToken)
    {
        var project = await _context.Projects
            .IgnoreQueryFilters()
            .Where(candidate => candidate.Id == request.ProjectId)
            .Select(candidate => new { candidate.Id, candidate.OrganizationId })
            .FirstOrDefaultAsync(cancellationToken);
        if (project is null)
        {
            throw new KeyNotFoundException($"Project with ID '{request.ProjectId}' was not found.");
        }
        await EnsureProjectAccess(request.ProjectId, project.OrganizationId, cancellationToken);

        var columns = await _context.BoardColumns
            .Where(column => column.BoardId == request.ProjectId)
            .OrderBy(column => column.Order)
            .ToListAsync(cancellationToken);

        if (columns.Count == 0)
        {
            var defaultColumnNames = new[] { "To Do", "In Progress", "Done" };
            columns = defaultColumnNames
                .Select((name, order) => new BoardColumn(Guid.NewGuid(), request.ProjectId, name, order))
                .ToList();
            _context.BoardColumns.AddRange(columns);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return columns
            .Select(column => new BoardColumnResponse(
                column.Id,
                request.ProjectId,
                column.Name,
                column.Order,
                DateTime.UtcNow))
            .ToList();
    }

    private async Task EnsureProjectAccess(Guid projectId, Guid organizationId, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User authentication context is missing.");
        var hasAccess = await _context.ProjectMembers.AnyAsync(
            member => member.ProjectId == projectId && member.UserId == userId,
            cancellationToken)
            || _currentUserService.ActiveOrgId == organizationId;
        if (!hasAccess)
        {
            throw new UnauthorizedAccessException("Project membership is required.");
        }
    }
}