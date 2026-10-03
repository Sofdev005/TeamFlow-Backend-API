using TeamFlow.Domain.Common;
using TeamFlow.Domain.Exceptions;

namespace TeamFlow.Domain.Entities;

public class Organization : AggregateRoot<Guid>
{
    public string Name { get; private set; } = default!;
    public string Slug { get; private set; } = default!;
    public string PlanTier { get; private set; } = "Free";
    public DateTime CreatedAt { get; private set; }

    private Organization() { } // EF Core

    public Organization(Guid id, string name, string slug, string planTier = "Free") 
        : base(id == Guid.Empty ? Guid.NewGuid() : id)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Organization name cannot be empty.");

        if (string.IsNullOrWhiteSpace(slug))
            throw new DomainException("Organization slug cannot be empty.");

        Name = name.Trim();
        Slug = slug.Trim().ToLowerInvariant();
        PlanTier = planTier;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new DomainException("Organization name cannot be empty.");

        Name = newName.Trim();
    }
}