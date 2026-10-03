using TeamFlow.Domain.Common;
using TeamFlow.Domain.Enums;

namespace TeamFlow.Domain.Entities;

public class ProjectMember : Entity<Guid>
{
    public Guid ProjectId { get; private set; }
    public Guid UserId { get; private set; }
    public OrgRole Role { get; private set; }
    public DateTime AddedAt { get; private set; }
    public DateTime? LastSeenAt { get; private set; }

    private ProjectMember() { }

    public ProjectMember(Guid id, Guid projectId, Guid userId, OrgRole role)
        : base(id == Guid.Empty ? Guid.NewGuid() : id)
    {
        ProjectId = projectId;
        UserId = userId;
        Role = role;
        AddedAt = DateTime.UtcNow;
    }

    public void UpdateRole(OrgRole role) => Role = role;

    public void RecordPresence(DateTime lastSeenAt) => LastSeenAt = lastSeenAt;
}