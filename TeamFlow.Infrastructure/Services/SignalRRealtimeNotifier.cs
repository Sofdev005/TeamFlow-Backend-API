using Microsoft.AspNetCore.SignalR;
using TeamFlow.Application.Common.Interfaces;

namespace TeamFlow.Infrastructure.Services;

public class SignalRRealtimeNotifier<THub> : IRealtimeNotifier where THub : Hub
{
    private readonly IHubContext<THub> _hubContext;

    public SignalRRealtimeNotifier(IHubContext<THub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyTaskMovedAsync(Guid projectId, Guid taskId, Guid targetColumnId, double newOrder, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients.Group($"project-{projectId}")
            .SendAsync("TaskMoved", new { TaskId = taskId, TargetColumnId = targetColumnId, NewOrder = newOrder }, cancellationToken);
    }

    public async Task NotifyTaskCreatedAsync(Guid projectId, Guid taskId, string title, Guid columnId, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients.Group($"project-{projectId}")
            .SendAsync("TaskCreated", new { TaskId = taskId, Title = title, ColumnId = columnId }, cancellationToken);
    }

    public async Task NotifyTaskUpdatedAsync(Guid projectId, Guid taskId, string title, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients.Group($"project-{projectId}")
            .SendAsync("TaskUpdated", new { TaskId = taskId, Title = title }, cancellationToken);
    }

    public async Task NotifyTaskDeletedAsync(Guid projectId, Guid taskId, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients.Group($"project-{projectId}")
            .SendAsync("TaskDeleted", new { TaskId = taskId }, cancellationToken);
    }
}