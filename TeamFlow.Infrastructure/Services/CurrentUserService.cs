using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using TeamFlow.Application.Common.Interfaces;

namespace TeamFlow.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier)
                              ?? _httpContextAccessor.HttpContext?.User?.FindFirstValue("sub");

            return Guid.TryParse(userIdClaim, out var parsedGuid) ? parsedGuid : null;
        }
    }

    public Guid? ActiveOrgId
    {
        get
        {
            var orgClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue("org_id");

            return Guid.TryParse(orgClaim, out var parsedGuid) ? parsedGuid : null;
        }
    }

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}