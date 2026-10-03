using TeamFlow.Domain.Common;

namespace TeamFlow.Domain.Entities;

public class TaskLabel : Entity<Guid>
{
    public Guid TaskId { get; private set; }
    public Guid LabelId { get; private set; }

    private TaskLabel() { } // EF Core

    public TaskLabel(Guid id, Guid taskId, Guid labelId)
        : base(id == Guid.Empty ? Guid.NewGuid() : id)
    {
        TaskId = taskId;
        LabelId = labelId;
    }
}