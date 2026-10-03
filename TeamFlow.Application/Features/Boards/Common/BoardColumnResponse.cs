namespace TeamFlow.Application.Features.Boards.Common;

public record BoardColumnResponse(
    Guid Id,
    Guid ProjectId,
    string Name,
    int Order,
    DateTime CreatedAt);