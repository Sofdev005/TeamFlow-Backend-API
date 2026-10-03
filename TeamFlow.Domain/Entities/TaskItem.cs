using TeamFlow.Domain.Common;
using TeamFlow.Domain.Enums;
using TeamFlow.Domain.Events;
using TeamFlow.Domain.Exceptions;

namespace TeamFlow.Domain.Entities;

public class TaskItem : AggregateRoot<Guid>
{
    public Guid ProjectId { get; private set; }
    public Guid BoardColumnId { get; private set; }
    public Guid? ParentTaskId { get; private set; }
    public string Title { get; private set; } = default!;
    public string? Description { get; private set; }
    public TaskPriority Priority { get; private set; }
    public Guid? AssigneeId { get; private set; }
    public Guid ReporterId { get; private set; }
    public DateTime? DueDate { get; private set; }
    public double Order { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private TaskItem() { } // EF Core

    public TaskItem(
        Guid id,
        Guid projectId,
        Guid boardColumnId,
        string title,
        Guid reporterId,
        TaskPriority priority = TaskPriority.Medium,
        double order = 0,
        Guid? parentTaskId = null)
        : base(id == Guid.Empty ? Guid.NewGuid() : id)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Task title cannot be empty.");

        ProjectId = projectId;
        BoardColumnId = boardColumnId;
        Title = title.Trim();
        ReporterId = reporterId;
        Priority = priority;
        Order = order;
        ParentTaskId = parentTaskId;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AssignTo(Guid assigneeId, Guid assignedByUserId)
    {
        AssigneeId = assigneeId;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new TaskAssignedEvent(Id, assigneeId, assignedByUserId));
    }

    public void SetDueDate(DateTime? dueDate)
    {
        if (dueDate.HasValue && dueDate.Value < DateTime.UtcNow.AddMinutes(-5))
            throw new DomainException("Due date cannot be in the past.");

        DueDate = dueDate;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MoveToColumn(Guid newColumnId, double newOrder)
    {
        BoardColumnId = newColumnId;
        Order = newOrder;
        UpdatedAt = DateTime.UtcNow;
    }
}