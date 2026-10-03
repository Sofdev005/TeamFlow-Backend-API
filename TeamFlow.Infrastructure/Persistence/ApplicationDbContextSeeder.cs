using Microsoft.EntityFrameworkCore;
using TeamFlow.Application.Common.Interfaces;
using TeamFlow.Domain.Entities;

namespace TeamFlow.Infrastructure.Persistence;

public static class ApplicationDbContextSeeder
{
    public static async Task SeedAsync(IApplicationDbContext context)
    {
        // Safe check to seed only if no organizations exist
        if (!await context.Organizations.AnyAsync())
        {
            // Optional: Add default seed logic here
            await context.SaveChangesAsync();
        }
    }
}