using MediatR;
using Microsoft.EntityFrameworkCore;
using TeamFlow.Application.Common.Interfaces;
using TeamFlow.Domain.Enums;

namespace TeamFlow.Application.Features.Organizations.Queries.GetMembers;

public record MemberResponse(
    Guid MemberId,
    Guid UserId,
    OrgRole Role,
    MemberStatus Status,
    DateTime JoinedAt,
    string Email,
    string FullName);

public record GetMembersQuery : IRequest<List<MemberResponse>>;

public class GetMembersQueryHandler : IRequestHandler<GetMembersQuery, List<MemberResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetMembersQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<List<MemberResponse>> Handle(GetMembersQuery request, CancellationToken cancellationToken)
    {
        var orgId = _currentUserService.ActiveOrgId
            ?? throw new UnauthorizedAccessException("Active organization context missing.");

        return await (
            from member in _context.OrganizationMembers.AsNoTracking()
            join user in _context.Users.AsNoTracking() on member.UserId equals user.Id
            where member.OrganizationId == orgId
            select new MemberResponse(
                member.Id,
                member.UserId,
                member.Role,
                member.Status,
                member.JoinedAt,
                user.Email,
                user.FirstName + " " + user.LastName))
            .ToListAsync(cancellationToken);
    }
}