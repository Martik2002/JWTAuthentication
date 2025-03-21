using JWTAuthentication.Common.Services;
using JWTAuthentication.Database;
using JWTAuthentication.Interfaces;
using JWTAuthentication.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


#region --service config

builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddTransient<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
builder.Services.AddTransient<IUserService, UsersService>();
builder.Services.AddTransient<IPasswordHasher, PasswordHasher>();
builder.Services.AddControllers();
builder.Services.AddDbContext<ApplicationDbContext>((options) =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>();

#endregion

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseSwaggerUI();
app.UseSwagger();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();