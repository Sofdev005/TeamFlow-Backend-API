using FluentAssertions;
using TeamFlow.Application.Features.Tasks.Commands.MoveTask;
using Xunit;

namespace TeamFlow.Tests.Application.Tasks;

public class MoveTaskCommandHandlerTests
{
    [Fact]
    public void MoveTaskCommand_ShouldInitializeCorrectly()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var targetColumnId = Guid.NewGuid();
        double newOrder = 1500.0;

        // Act - 3 parameters matching your MoveTaskCommand definition
        var command = new MoveTaskCommand(taskId, targetColumnId, newOrder);

        // Assert
        command.TaskId.Should().Be(taskId);
        command.TargetColumnId.Should().Be(targetColumnId);
        command.NewOrder.Should().Be(newOrder);
    }
}