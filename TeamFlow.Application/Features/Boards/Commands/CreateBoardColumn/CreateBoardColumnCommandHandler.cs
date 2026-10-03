using MediatR;
using Microsoft.EntityFrameworkCore;
using TeamFlow.Application.Common.Interfaces;
using TeamFlow.Application.Features.Boards.Common;
using TeamFlow.Domain.Entities;

namespace TeamFlow.Application.Features.Boards.Commands.CreateBoardColumn;

public class CreateBoardColumnCommandHandler : IRequestHandler<CreateBoardColumnCommand, BoardColumnResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CreateBoardColumnCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<BoardColumnResponse> Handle(
        CreateBoardColumnCommand request,
        CancellationToken cancellationToken)
    {
        var project = await _context.Projects
            .IgnoreQueryFilters()
            .Where(candidate => candidate.Id == request.ProjectId)
            .Select(candidate => new { candidate.Id, candidate.OrganizationId })
            .FirstOrDefaultAsync(cancellationToken);
        if (project is null)
        {
            throw new KeyNotFoundException($"Project with ID '{request.ProjectId}' was not found.");
        }
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User authentication context is missing.");
        var hasAccess = await _context.ProjectMembers.AnyAsync(
            member => member.ProjectId == request.ProjectId && member.UserId == userId,
            cancellationToken)
            || _currentUserService.ActiveOrgId == project.OrganizationId;
        if (!hasAccess) throw new UnauthorizedAccessException("Project membership is required.");

        var column = new BoardColumn(Guid.NewGuid(), request.ProjectId, request.Name, request.Order);
        _context.BoardColumns.Add(column);
        await _context.SaveChangesAsync(cancellationToken);

        return new BoardColumnResponse(
            column.Id,
            request.ProjectId,
            column.Name,
            column.Order,
            DateTime.UtcNow);
    }
}