using MediatR;
using TeamFlow.Application.Features.Tasks.Common;

namespace TeamFlow.Application.Features.Tasks.Queries.GetTasks;

public record GetTasksQuery(Guid ProjectId) : IRequest<List<TaskResponse>>;