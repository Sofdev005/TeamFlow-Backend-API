using TeamFlow.Domain.Entities;
using TeamFlow.Domain.Enums;
using TeamFlow.Domain.Exceptions;
using Xunit;

namespace TeamFlow.Domain.Tests;

public class TaskItemTests
{
    [Fact]
    public void SetDueDate_InPast_ThrowsDomainException()
    {
        // Arrange
        var task = new TaskItem(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Test Task", Guid.NewGuid());
        var pastDate = DateTime.UtcNow.AddDays(-1);

        // Act & Assert
        Assert.Throws<DomainException>(() => task.SetDueDate(pastDate));
    }

    [Fact]
    public void AssignTo_EmitsTaskAssignedEvent()
    {
        // Arrange
        var task = new TaskItem(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Test Task", Guid.NewGuid());
        var assigneeId = Guid.NewGuid();
        var assignerId = Guid.NewGuid();

        // Act
        task.AssignTo(assigneeId, assignerId);

        // Assert
        Assert.Single(task.DomainEvents);
        Assert.NotNull(task.AssigneeId);
        Assert.Equal(assigneeId, task.AssigneeId);
    }
}