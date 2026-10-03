using TeamFlow.Domain.Common;
using TeamFlow.Domain.Exceptions;

namespace TeamFlow.Domain.Entities;

public class BoardColumn : Entity<Guid>
{
    public Guid BoardId { get; private set; }
    public string Name { get; private set; } = default!;
    public int Order { get; private set; }

    private BoardColumn() { } // EF Core

    public BoardColumn(Guid id, Guid boardId, string name, int order)
        : base(id == Guid.Empty ? Guid.NewGuid() : id)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Column name cannot be empty.");

        BoardId = boardId;
        Name = name.Trim();
        Order = order;
    }

    public void UpdateOrder(int newOrder) => Order = newOrder;
}