using TeamFlow.Domain.Common;
using TeamFlow.Domain.Exceptions;

namespace TeamFlow.Domain.Entities;

public class Comment : AggregateRoot<Guid>
{
    public Guid TaskId { get; private set; }
    public Guid AuthorId { get; private set; }
    public string Body { get; private set; } = default!;
    public DateTime CreatedAt { get; private set; }
    public DateTime? EditedAt { get; private set; }

    private Comment() { } // EF Core

    public Comment(Guid id, Guid taskId, Guid authorId, string body)
        : base(id == Guid.Empty ? Guid.NewGuid() : id)
    {
        if (string.IsNullOrWhiteSpace(body))
            throw new DomainException("Comment body cannot be empty.");

        TaskId = taskId;
        AuthorId = authorId;
        Body = body.Trim();
        CreatedAt = DateTime.UtcNow;
    }

    public void Edit(string newBody)
    {
        if (string.IsNullOrWhiteSpace(newBody))
            throw new DomainException("Comment body cannot be empty.");

        Body = newBody.Trim();
        EditedAt = DateTime.UtcNow;
    }
}