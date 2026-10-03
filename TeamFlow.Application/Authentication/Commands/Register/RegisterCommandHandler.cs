using MediatR;
using Microsoft.EntityFrameworkCore;
using TeamFlow.Application.Authentication.Common;
using TeamFlow.Application.Common.Interfaces;
using TeamFlow.Application.Common.Interfaces.Authentication;
using TeamFlow.Domain.Entities;
using TeamFlow.Domain.Enums;

namespace TeamFlow.Application.Authentication.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthenticationResult>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public RegisterCommandHandler(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<AuthenticationResult> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await _context.Users
            .AnyAsync(u => u.Email == request.Email, cancellationToken);

        if (existingUser)
        {
            throw new InvalidOperationException("User with this email already exists.");
        }

        // Generate URL-friendly slug from Organization Name
        var slug = request.OrganizationName.Trim().ToLowerInvariant().Replace(" ", "-");

        // Instantiate Organization using its domain constructor
        var organization = new Organization(
            id: Guid.NewGuid(),
            name: request.OrganizationName,
            slug: slug
        );

        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            OrganizationId = organization.Id,
            CreatedAt = DateTime.UtcNow
        };
        var ownerMembership = new OrganizationMember(
            id: Guid.NewGuid(),
            organizationId: organization.Id,
            userId: user.Id,
            role: OrgRole.Owner,
            status: MemberStatus.Active);

        _context.Organizations.Add(organization);
        _context.Users.Add(user);
        _context.OrganizationMembers.Add(ownerMembership);
        await _context.SaveChangesAsync(cancellationToken);

        var token = _jwtTokenGenerator.GenerateToken(user.Id, user.FirstName, user.LastName, user.Email, organization.Id);

        return new AuthenticationResult(user.Id, user.FirstName, user.LastName, user.Email, organization.Id, token);
    }
}