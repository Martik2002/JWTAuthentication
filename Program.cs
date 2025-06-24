using JWTAuthentication;
using JWTAuthentication.Common.Models;
using JWTAuthentication.Database;
using JWTAuthentication.Infrastructure;


var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JwtSettings"));

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();


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

app.UseAuthentication();
app.UseAuthorization();

app.MapEndpoints();

using (var scope = app.Services.CreateScope())
{
    var dbContextInitialiser = scope.ServiceProvider.GetRequiredService<IdentityDataInitializer>();
    await dbContextInitialiser.InitialiseAsync(app);
}

app.Run();