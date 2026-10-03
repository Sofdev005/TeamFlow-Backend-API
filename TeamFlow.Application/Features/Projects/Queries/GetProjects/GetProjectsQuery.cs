using MediatR;
using Microsoft.EntityFrameworkCore;
using TeamFlow.Application.Common.Interfaces;

namespace TeamFlow.Application.Features.Projects.Queries.GetProjects;

public record GetProjectsQuery : IRequest<List<ProjectResponse>>;

public class GetProjectsQueryHandler : IRequestHandler<GetProjectsQuery, List<ProjectResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetProjectsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<List<ProjectResponse>> Handle(GetProjectsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User authentication context is missing.");
        var activeOrgId = _currentUserService.ActiveOrgId;

        return await _context.Projects
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(project =>
                _context.ProjectMembers.Any(member => member.ProjectId == project.Id && member.UserId == userId)
                || (activeOrgId != null
                    && project.OrganizationId == activeOrgId
                    && !_context.ProjectMembers.Any(member => member.ProjectId == project.Id)))
            .Select(p => new ProjectResponse(
                p.Id,
                p.OrganizationId,
                p.TeamId,
                p.Name,
                p.Description,
                p.IsArchived,
                p.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}