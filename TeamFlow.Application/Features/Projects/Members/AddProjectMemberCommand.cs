using MediatR;
using Microsoft.EntityFrameworkCore;
using TeamFlow.Application.Common.Interfaces;
using TeamFlow.Domain.Enums;

namespace TeamFlow.Application.Features.Projects.Members;

public record AddProjectMemberCommand(Guid ProjectId, Guid UserId, OrgRole Role) : IRequest<Guid>;

public class AddProjectMemberCommandHandler : IRequestHandler<AddProjectMemberCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public AddProjectMemberCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Guid> Handle(AddProjectMemberCommand request, CancellationToken cancellationToken)
    {
        var project = await _context.Projects
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(item => item.Id == request.ProjectId, cancellationToken);
        if (project is null)
        {
            throw new KeyNotFoundException($"Project with ID '{request.ProjectId}' was not found.");
        }

        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User authentication context is missing.");
        var canManageMembers = await _context.ProjectMembers.AnyAsync(
            member => member.ProjectId == request.ProjectId && member.UserId == currentUserId,
            cancellationToken)
            || _currentUserService.ActiveOrgId == project.OrganizationId;
        if (!canManageMembers) throw new UnauthorizedAccessException("Project membership is required.");

        var isOrganizationMember = await _context.OrganizationMembers.AnyAsync(
            member => member.OrganizationId == project.OrganizationId && member.UserId == request.UserId,
            cancellationToken);
        if (!isOrganizationMember)
        {
            throw new KeyNotFoundException("Add this person to the organization before assigning them to a project.");
        }

        var projectMember = await _context.ProjectMembers.FirstOrDefaultAsync(
            member => member.ProjectId == request.ProjectId && member.UserId == request.UserId,
            cancellationToken);
        if (projectMember is not null)
        {
            projectMember.UpdateRole(request.Role);
        }
        else
        {
            projectMember = new Domain.Entities.ProjectMember(
                Guid.NewGuid(), request.ProjectId, request.UserId, request.Role);
            _context.ProjectMembers.Add(projectMember);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return projectMember.Id;
    }
}