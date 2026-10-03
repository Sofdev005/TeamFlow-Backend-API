namespace TeamFlow.Application.Features.Tasks.Common;

public record TaskResponse(
    Guid Id,
    Guid ProjectId,
    Guid BoardColumnId,
    string Title,
    string? Description,
    string Priority,
    Guid? AssigneeId,
    Guid ReporterId,
    DateTime? DueDate,
    double Order,
    DateTime CreatedAt,
    DateTime UpdatedAt);