using MediatR;
using TeamFlow.Domain.Enums;

namespace TeamFlow.Application.Features.Organizations.Commands.InviteMember;

public record InviteMemberCommand(
    string Email,
    OrgRole Role) : IRequest<Guid>;