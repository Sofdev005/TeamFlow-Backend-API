namespace TeamFlow.Application.Common.Interfaces;

public interface IRealtimeNotifier
{
    Task NotifyTaskMovedAsync(Guid projectId, Guid taskId, Guid targetColumnId, double newOrder, CancellationToken cancellationToken = default);
    Task NotifyTaskCreatedAsync(Guid projectId, Guid taskId, string title, Guid columnId, CancellationToken cancellationToken = default);
    Task NotifyTaskUpdatedAsync(Guid projectId, Guid taskId, string title, CancellationToken cancellationToken = default);
    Task NotifyTaskDeletedAsync(Guid projectId, Guid taskId, CancellationToken cancellationToken = default);
}