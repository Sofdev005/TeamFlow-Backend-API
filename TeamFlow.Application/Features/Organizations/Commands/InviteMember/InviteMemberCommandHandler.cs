using MediatR;
using Microsoft.EntityFrameworkCore;
using TeamFlow.Application.Common.Interfaces;
using TeamFlow.Domain.Entities;
using TeamFlow.Domain.Enums;
using TeamFlow.Domain.Exceptions;

namespace TeamFlow.Application.Features.Organizations.Commands.InviteMember;

public class InviteMemberCommandHandler : IRequestHandler<InviteMemberCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public InviteMemberCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Guid> Handle(InviteMemberCommand request, CancellationToken cancellationToken)
    {
        var orgId = _currentUserService.ActiveOrgId
            ?? throw new UnauthorizedAccessException("Active organization context missing.");

        var normalizedEmail = request.Email.Trim().ToUpperInvariant();
        var existingUser = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email.ToUpper() == normalizedEmail, cancellationToken);

        if (existingUser is null)
        {
            throw new KeyNotFoundException($"User with email '{request.Email}' not found.");
        }

        var member = await _context.OrganizationMembers
            .FirstOrDefaultAsync(m => m.OrganizationId == orgId && m.UserId == existingUser.Id, cancellationToken);

        if (member is not null)
        {
            member.UpdateRole(request.Role);
            member.UpdateStatus(MemberStatus.Active);
            await _context.SaveChangesAsync(cancellationToken);
            return member.Id;
        }

        member = new OrganizationMember(
            id: Guid.NewGuid(),
            organizationId: orgId,
            userId: existingUser.Id,
            role: request.Role,
            status: MemberStatus.Active,
            invitedByUserId: _currentUserService.UserId
        );

        _context.OrganizationMembers.Add(member);
        await _context.SaveChangesAsync(cancellationToken);

        return member.Id;
    }
}