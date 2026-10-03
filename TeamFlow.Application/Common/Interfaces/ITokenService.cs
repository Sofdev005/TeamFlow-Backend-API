using TeamFlow.Domain.Entities;

namespace TeamFlow.Application.Common.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(User user, Guid activeOrgId, string role);
    string GenerateRefreshToken();
    string HashToken(string token);
}