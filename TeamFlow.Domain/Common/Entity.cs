namespace TeamFlow.Domain.Common;

public abstract class Entity<TId>
{
    public TId Id { get; protected set; } = default!;

    protected Entity(TId id)
    {
        Id = id;
    }

    // Required for EF Core materialization
    protected Entity() { }

    public override bool Equals(object? obj)
    {
        if (obj is not Entity<TId> other)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (EqualityContract != other.EqualityContract)
            return false;

        return Id?.Equals(other.Id) ?? false;
    }

    public override int GetHashCode()
    {
        return (EqualityContract, Id).GetHashCode();
    }

    protected virtual Type EqualityContract => GetType();
}