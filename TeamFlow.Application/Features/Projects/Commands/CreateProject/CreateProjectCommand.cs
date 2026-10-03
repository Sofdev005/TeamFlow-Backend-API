using MediatR;

namespace TeamFlow.Application.Features.Projects.Commands.CreateProject;

public record CreateProjectCommand(
    string Name,
    string? Description = null,
    Guid? TeamId = null) : IRequest<ProjectResponse>;