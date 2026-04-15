using Scalar.AspNetCore;
using UserManagement.Infrastructure;
using UserManagement.Infrastructure.Data;
using UserManagement.Infrastructure.Seeds;

var builder = WebApplication.CreateBuilder(args);

// ── Services ────────────────────────────────────────────

// Pass the environment so DependencyInjection.cs can skip
// SQL Server when running integration tests
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebProject", policy =>
        policy.WithOrigins("https://localhost:7227", "http://localhost:5135")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// ── App Pipeline ────────────────────────────────────────
var app = builder.Build();

// Skip seeding during integration tests — the test factory
// seeds its own in-memory database instead
if (!builder.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await DatabaseSeeder.SeedAsync(context);
}

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing"))
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Title = "User Management API";
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowWebProject");
app.UseAuthorization();
app.MapControllers();

app.Run();

// Required for WebApplicationFactory in integration tests
public partial class Program { }