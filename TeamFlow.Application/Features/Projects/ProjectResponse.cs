namespace TeamFlow.Application.Features.Projects;

public record ProjectResponse(
    Guid Id,
    Guid OrganizationId,
    Guid? TeamId,
    string Name,
    string? Description,
    bool IsArchived,
    DateTime CreatedAt);