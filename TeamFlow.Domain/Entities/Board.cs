using TeamFlow.Domain.Common;
using TeamFlow.Domain.Exceptions;

namespace TeamFlow.Domain.Entities;

public class Board : AggregateRoot<Guid>
{
    public Guid ProjectId { get; private set; }
    public string Name { get; private set; } = default!;

    private Board() { } // EF Core

    public Board(Guid id, Guid projectId, string name)
        : base(id == Guid.Empty ? Guid.NewGuid() : id)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Board name cannot be empty.");

        ProjectId = projectId;
        Name = name.Trim();
    }
}