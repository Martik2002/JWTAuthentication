using JWTAuthentication;
using JWTAuthentication.Database;
using JWTAuthentication.Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configuration is loaded by CreateBuilder in this order:
// appsettings.json -> appsettings.{Environment}.json -> user secrets (Development) -> environment variables.
// Secrets (JwtSettings:Key, ConnectionStrings:DefaultConnection) must come from user secrets or environment variables.

builder.Host.UseSerilog((context, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("Logs/log.txt", rollingInterval: RollingInterval.Day));

builder.Services.AddApplicationServices(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHsts();
}

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapEndpoints();

await using (var scope = app.Services.CreateAsyncScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
    await initializer.InitialiseAsync();
}

app.Run();

// Makes the Program class visible to WebApplicationFactory in the integration tests
public partial class Program { }
