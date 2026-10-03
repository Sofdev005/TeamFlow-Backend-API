using MediatR;
using TeamFlow.Application.Authentication.Common;

namespace TeamFlow.Application.Authentication.Queries.Login;

public record LoginQuery(string Email, string Password) : IRequest<AuthenticationResult>;