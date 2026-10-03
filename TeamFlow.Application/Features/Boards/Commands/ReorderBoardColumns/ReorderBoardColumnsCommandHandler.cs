using MediatR;
using Microsoft.EntityFrameworkCore;
using TeamFlow.Application.Common.Interfaces;

namespace TeamFlow.Application.Features.Boards.Commands.ReorderBoardColumns;

public class ReorderBoardColumnsCommandHandler : IRequestHandler<ReorderBoardColumnsCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public ReorderBoardColumnsCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(ReorderBoardColumnsCommand request, CancellationToken cancellationToken)
    {
        var columns = await _context.BoardColumns
            .Where(c => c.BoardId == request.BoardId)
            .ToListAsync(cancellationToken);

        foreach (var orderDto in request.ColumnOrders)
        {
            var column = columns.FirstOrDefault(c => c.Id == orderDto.ColumnId);
            if (column is not null)
            {
                column.UpdateOrder(orderDto.NewOrder);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}