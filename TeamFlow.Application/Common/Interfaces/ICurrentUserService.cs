namespace TeamFlow.Application.Common.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    Guid? ActiveOrgId { get; }
    bool IsAuthenticated { get; }
}