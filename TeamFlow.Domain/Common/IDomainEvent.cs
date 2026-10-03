namespace TeamFlow.Domain.Common;

public interface IDomainEvent
{
    DateTime OccurredOn { get; }
}