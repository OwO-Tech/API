using API.Data;
using API.Entities;
using API.Extensions;
using API.Middleware;
using API.SignalR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddIdentityServices(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMiddleware<ExceptionMiddleware>();

// if (app.Environment.IsDevelopment())
// {
app.UseSwagger();
app.UseSwaggerUI();
// }

app.UseHttpsRedirection();
app.UseCors(builder => builder.AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()
        .WithOrigins("https://localhost:4200", "https://localhost:4200/*", "https://datingappgui2023.azurewebsites.net", "https://datingappgui2023.azurewebsites.net/*")
        );

app.UseAuthentication();
app.UseAuthorization();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();
app.MapHub<PresenceHub>("hubs/presence");
app.MapHub<MessageHub>("hubs/message");
app.MapFallbackToController("Index", "Fallback");

using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;

try
{
    DataContext context = services.GetRequiredService<DataContext>();
    UserManager<AppUser> userManager = services.GetRequiredService<UserManager<AppUser>>();
    RoleManager<AppRole> roleManager = services.GetRequiredService<RoleManager<AppRole>>();
    await context.Database.MigrateAsync();
    await Seed.ClearConnections(context);
    await Seed.SeedUsers(userManager, roleManager);
}
catch (System.Exception ex)
{
    var logger = services.GetService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred during migration");
}

app.Run();
