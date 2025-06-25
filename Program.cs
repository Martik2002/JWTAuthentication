using JWTAuthentication;
using JWTAuthentication.Common.Behaviours;
using JWTAuthentication.Common.Models;
using JWTAuthentication.Database;
using JWTAuthentication.Infrastructure;
using Serilog;


var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JwtSettings"));

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// added logger for file
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information() //you can change minimum leval
    .WriteTo.File("Logs/log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddLogging(loggingBuilder =>
    loggingBuilder.AddSerilog(dispose: true));

builder.Services.AddRequestTimeouts();

#region --service config

builder.Services.ServiceProvider(builder.Configuration);
builder.Services.AddScoped<IdentityDataInitializer>();
#endregion

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerUI();
    app.UseSwagger();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}


app.UseHttpsRedirection();

app.UseStaticFiles();


app.UseRouting();

app.UseMiddleware<UnhandledExceptionBehaviour>();

app.UseAuthentication();
app.UseAuthorization();

app.MapEndpoints();

using (var scope = app.Services.CreateScope())
{
    var dbContextInitialiser = scope.ServiceProvider.GetRequiredService<IdentityDataInitializer>();
    await dbContextInitialiser.InitialiseAsync(app);
}

app.Run();