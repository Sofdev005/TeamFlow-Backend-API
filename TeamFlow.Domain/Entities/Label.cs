using TeamFlow.Domain.Common;
using TeamFlow.Domain.Exceptions;

namespace TeamFlow.Domain.Entities;

public class Label : Entity<Guid>
{
    public Guid OrganizationId { get; private set; }
    public string Name { get; private set; } = default!;
    public string Color { get; private set; } = default!;

    private Label() { } // EF Core

    public Label(Guid id, Guid organizationId, string name, string color)
        : base(id == Guid.Empty ? Guid.NewGuid() : id)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Label name is required.");

        OrganizationId = organizationId;
        Name = name.Trim();
        Color = color;
    }
}