using MediatR;

namespace TeamFlow.Application.Features.Boards.Commands.ReorderBoardColumns;

public record ColumnOrderDto(Guid ColumnId, int NewOrder);

public record ReorderBoardColumnsCommand(
    Guid BoardId,
    List<ColumnOrderDto> ColumnOrders) : IRequest<Unit>;