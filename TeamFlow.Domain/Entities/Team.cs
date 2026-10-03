using TeamFlow.Domain.Common;
using TeamFlow.Domain.Exceptions;

namespace TeamFlow.Domain.Entities;

public class Team : AggregateRoot<Guid>
{
    public Guid OrganizationId { get; private set; }
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }

    private Team() { } // EF Core

    public Team(Guid id, Guid organizationId, string name, string? description = null)
        : base(id == Guid.Empty ? Guid.NewGuid() : id)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Team name cannot be empty.");

        OrganizationId = organizationId;
        Name = name.Trim();
        Description = description?.Trim();
    }

    public void UpdateDetails(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Team name cannot be empty.");

        Name = name.Trim();
        Description = description?.Trim();
    }
}