using MediatR;
using Microsoft.EntityFrameworkCore;
using TeamFlow.Application.Common.Interfaces;
using TeamFlow.Domain.Enums;

namespace TeamFlow.Application.Features.Projects.Members;

public record ProjectMemberResponse(
    Guid UserId,
    string Email,
    string FullName,
    OrgRole Role,
    bool IsOnline);

public record GetProjectMembersQuery(Guid ProjectId) : IRequest<List<ProjectMemberResponse>>;

public class GetProjectMembersQueryHandler : IRequestHandler<GetProjectMembersQuery, List<ProjectMemberResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetProjectMembersQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<List<ProjectMemberResponse>> Handle(
        GetProjectMembersQuery request,
        CancellationToken cancellationToken)
    {
        var project = await _context.Projects
            .IgnoreQueryFilters()
            .Where(item => item.Id == request.ProjectId)
            .Select(item => new { item.Id, item.OrganizationId })
            .FirstOrDefaultAsync(cancellationToken);
        if (project is null)
        {
            throw new KeyNotFoundException($"Project with ID '{request.ProjectId}' was not found.");
        }

        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User authentication context is missing.");
        var isAssigned = await _context.ProjectMembers.AnyAsync(
            member => member.ProjectId == request.ProjectId && member.UserId == userId,
            cancellationToken);
        if (!isAssigned && _currentUserService.ActiveOrgId != project.OrganizationId)
        {
            throw new UnauthorizedAccessException("Project membership is required.");
        }

        var hasAssignments = await _context.ProjectMembers
            .AnyAsync(member => member.ProjectId == request.ProjectId, cancellationToken);
        if (!hasAssignments)
        {
            var workspaceMembers = await _context.OrganizationMembers
                .Where(member => member.OrganizationId == project.OrganizationId)
                .ToListAsync(cancellationToken);
            _context.ProjectMembers.AddRange(workspaceMembers.Select(member =>
                new Domain.Entities.ProjectMember(
                    Guid.NewGuid(), request.ProjectId, member.UserId, member.Role)));
            await _context.SaveChangesAsync(cancellationToken);
        }

        var onlineCutoff = DateTime.UtcNow.AddSeconds(-45);
        return await (
            from projectMember in _context.ProjectMembers.AsNoTracking()
            join user in _context.Users.AsNoTracking() on projectMember.UserId equals user.Id
            where projectMember.ProjectId == request.ProjectId
            orderby user.FirstName, user.LastName
            select new ProjectMemberResponse(
                user.Id,
                user.Email,
                user.FirstName + " " + user.LastName,
                projectMember.Role,
                projectMember.LastSeenAt.HasValue && projectMember.LastSeenAt.Value >= onlineCutoff))
            .ToListAsync(cancellationToken);
    }
}

public record RecordProjectPresenceCommand(Guid ProjectId) : IRequest<Unit>;

public class RecordProjectPresenceCommandHandler : IRequestHandler<RecordProjectPresenceCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public RecordProjectPresenceCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Unit> Handle(RecordProjectPresenceCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User authentication context is missing.");
        var member = await _context.ProjectMembers.FirstOrDefaultAsync(
            item => item.ProjectId == request.ProjectId && item.UserId == userId,
            cancellationToken);
        if (member is null)
        {
            throw new KeyNotFoundException("Project membership is required to report presence.");
        }

        member.RecordPresence(DateTime.UtcNow);
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}