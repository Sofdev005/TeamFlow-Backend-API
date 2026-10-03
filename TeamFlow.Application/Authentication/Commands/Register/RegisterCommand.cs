using MediatR;
using TeamFlow.Application.Authentication.Common;

namespace TeamFlow.Application.Authentication.Commands.Register;

public record RegisterCommand(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string OrganizationName) : IRequest<AuthenticationResult>;