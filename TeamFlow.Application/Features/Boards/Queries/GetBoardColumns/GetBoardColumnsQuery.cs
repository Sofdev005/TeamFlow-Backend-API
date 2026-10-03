using MediatR;
using TeamFlow.Application.Features.Boards.Common; // or TeamFlow.Application.Features.Boards depending on location

namespace TeamFlow.Application.Features.Boards.Queries.GetBoardColumns;

public record GetBoardColumnsQuery(Guid ProjectId) : IRequest<List<BoardColumnResponse>>;