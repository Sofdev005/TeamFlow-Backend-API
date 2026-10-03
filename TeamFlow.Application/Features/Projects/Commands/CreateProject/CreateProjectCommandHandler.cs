using MediatR;
using TeamFlow.Application.Common.Interfaces;
using TeamFlow.Domain.Entities;

namespace TeamFlow.Application.Features.Projects.Commands.CreateProject;

public class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, ProjectResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CreateProjectCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<ProjectResponse> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        var orgId = _currentUserService.ActiveOrgId
            ?? throw new UnauthorizedAccessException("Tenant organization context is missing.");
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User authentication context is missing.");

        var project = new Project(
            id: Guid.NewGuid(),
            organizationId: orgId,
            name: request.Name,
            teamId: request.TeamId,
            description: request.Description
        );

        _context.Projects.Add(project);
        _context.ProjectMembers.Add(new ProjectMember(
            Guid.NewGuid(), project.Id, userId, TeamFlow.Domain.Enums.OrgRole.Owner));
        await _context.SaveChangesAsync(cancellationToken);

        return new ProjectResponse(
            project.Id,
            project.OrganizationId,
            project.TeamId,
            project.Name,
            project.Description,
            project.IsArchived,
            project.CreatedAt);
    }
}