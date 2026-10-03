using MediatR;
using TeamFlow.Application.Features.Boards.Common;

namespace TeamFlow.Application.Features.Boards.Commands.CreateBoardColumn;

public record CreateBoardColumnCommand(
    Guid ProjectId,
    string Name,
    int Order) : IRequest<BoardColumnResponse>;