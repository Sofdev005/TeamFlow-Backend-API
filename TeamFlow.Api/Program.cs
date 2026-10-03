using Microsoft.EntityFrameworkCore;
using TeamFlow.Application;
using TeamFlow.Infrastructure;
using TeamFlow.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register Clean Architecture layers
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddPersistenceServices(builder.Configuration);

// CORS Policy for Next.js Frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Automatically create tables & seed database on container startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var context = services.GetRequiredService<TeamFlowDbContext>();

            var appliedMigrations = await context.Database.GetAppliedMigrationsAsync();
            var knownMigrations = context.Database.GetMigrations();
            var hasUnknownBaseline = appliedMigrations.Any(
                migration => !knownMigrations.Contains(migration, StringComparer.Ordinal));

            if (hasUnknownBaseline)
            {
                logger.LogWarning(
                    "Skipping automatic migrations because the database has an unrecognized migration baseline: {Migrations}",
                    string.Join(", ", appliedMigrations));
            }
            else
            {
                await context.Database.MigrateAsync();
            }

            await context.Database.ExecuteSqlRawAsync("""
                CREATE TABLE IF NOT EXISTS "ProjectMembers" (
                    "Id" uuid PRIMARY KEY,
                    "ProjectId" uuid NOT NULL,
                    "UserId" uuid NOT NULL,
                    "Role" integer NOT NULL,
                    "AddedAt" timestamp with time zone NOT NULL,
                    "LastSeenAt" timestamp with time zone NULL
                );
                """);
            await context.Database.ExecuteSqlRawAsync("""
                CREATE UNIQUE INDEX IF NOT EXISTS "IX_ProjectMembers_ProjectId_UserId"
                ON "ProjectMembers" ("ProjectId", "UserId");
                """);

        await ApplicationDbContextSeeder.SeedAsync(context);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();