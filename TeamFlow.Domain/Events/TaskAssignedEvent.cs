using TeamFlow.Domain.Common;

namespace TeamFlow.Domain.Events;

public record TaskAssignedEvent(Guid TaskId, Guid AssigneeId, Guid AssignedByUserId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}