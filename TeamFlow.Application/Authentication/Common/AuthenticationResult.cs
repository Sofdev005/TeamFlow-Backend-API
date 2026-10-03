namespace TeamFlow.Application.Authentication.Common;

public record AuthenticationResult(
    Guid UserId,
    string FirstName,
    string LastName,
    string Email,
    Guid OrgId,
    string Token);