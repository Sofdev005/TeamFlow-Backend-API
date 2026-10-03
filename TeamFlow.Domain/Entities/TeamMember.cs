using TeamFlow.Domain.Common;

namespace TeamFlow.Domain.Entities;

public class TeamMember : Entity<Guid>
{
    public Guid TeamId { get; private set; }
    public Guid UserId { get; private set; }

    private TeamMember() { } // EF Core

    public TeamMember(Guid id, Guid teamId, Guid userId)
        : base(id == Guid.Empty ? Guid.NewGuid() : id)
    {
        TeamId = teamId;
        UserId = userId;
    }
}