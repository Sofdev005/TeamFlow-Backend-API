using TeamFlow.Domain.Common;
using TeamFlow.Domain.Exceptions;

namespace TeamFlow.Domain.Entities;

public class Project : AggregateRoot<Guid>
{
    public Guid OrganizationId { get; private set; }
    public Guid? TeamId { get; private set; }
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public bool IsArchived { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Project() { } // EF Core

    public Project(Guid id, Guid organizationId, string name, Guid? teamId = null, string? description = null)
        : base(id == Guid.Empty ? Guid.NewGuid() : id)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Project name cannot be empty.");

        OrganizationId = organizationId;
        Name = name.Trim();
        TeamId = teamId;
        Description = description?.Trim();
        IsArchived = false;
        CreatedAt = DateTime.UtcNow;
    }

    public void Archive() => IsArchived = true;
    public void Unarchive() => IsArchived = false;
}