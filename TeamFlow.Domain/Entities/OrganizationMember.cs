using TeamFlow.Domain.Common;
using TeamFlow.Domain.Enums;
using TeamFlow.Domain.Exceptions;

namespace TeamFlow.Domain.Entities;

public class OrganizationMember : Entity<Guid>
{
    public Guid OrganizationId { get; private set; }
    public Guid UserId { get; private set; }
    public OrgRole Role { get; private set; }
    public MemberStatus Status { get; private set; }
    public Guid? InvitedByUserId { get; private set; }
    public DateTime JoinedAt { get; private set; }

    private OrganizationMember() { } // EF Core

    public OrganizationMember(
        Guid id,
        Guid organizationId,
        Guid userId,
        OrgRole role,
        MemberStatus status = MemberStatus.Active,
        Guid? invitedByUserId = null) 
        : base(id == Guid.Empty ? Guid.NewGuid() : id)
    {
        OrganizationId = organizationId;
        UserId = userId;
        Role = role;
        Status = status;
        InvitedByUserId = invitedByUserId;
        JoinedAt = DateTime.UtcNow;
    }

    public void UpdateRole(OrgRole newRole)
    {
        Role = newRole;
    }

    public void UpdateStatus(MemberStatus newStatus)
    {
        Status = newStatus;
    }
}