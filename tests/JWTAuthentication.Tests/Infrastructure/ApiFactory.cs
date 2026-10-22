using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;

namespace JWTAuthentication.Tests.Infrastructure;

/// <summary>
/// Starts the real API in memory against a real PostgreSQL running in Docker,
/// so the tests cover the full stack: HTTP pipeline, auth, EF Core, migrations.
/// </summary>
public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string AdminUserName = "admin";
    public const string AdminPassword = "Admin123!Test";

    private readonly PostgreSqlContainer _database = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .Build();

    public async Task InitializeAsync()
    {
        await _database.StartAsync();

        // Program reads these while building the host, so they must exist before the
        // first client is created. Environment variables are visible to CreateBuilder.
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", _database.GetConnectionString());
        Environment.SetEnvironmentVariable("JwtSettings__Key", "integration-tests-signing-key-that-is-long-enough-for-hmac-sha512-0123456789");
        Environment.SetEnvironmentVariable("Seed__AdminUserName", AdminUserName);
        Environment.SetEnvironmentVariable("Seed__AdminPassword", AdminPassword);
        Environment.SetEnvironmentVariable("RateLimiting__AuthPermitLimit", "10000");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        await _database.DisposeAsync();
    }
}

[CollectionDefinition(Name)]
public sealed class ApiCollection : ICollectionFixture<ApiFactory>
{
    public const string Name = "Api";
}
