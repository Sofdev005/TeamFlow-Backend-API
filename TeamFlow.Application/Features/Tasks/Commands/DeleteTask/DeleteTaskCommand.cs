using MediatR;

namespace TeamFlow.Application.Features.Tasks.Commands.DeleteTask;

public record DeleteTaskCommand(Guid Id) : IRequest<Unit>;