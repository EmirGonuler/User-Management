using Scalar.AspNetCore;
using UserManagement.Infrastructure;
using UserManagement.Infrastructure.Data;
using UserManagement.Infrastructure.Seeds;

var builder = WebApplication.CreateBuilder(args);

// ── Services ────────────────────────────────────────────
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();

// .NET 10 built-in OpenAPI support
builder.Services.AddOpenApi();

// Allow the MVC Web project to call this API (CORS)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebProject", policy =>
        policy.WithOrigins("https://localhost:7001", "http://localhost:5001")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// ── App Pipeline ────────────────────────────────────────
var app = builder.Build();

// Seed the database with initial data on startup
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await DatabaseSeeder.SeedAsync(context);
}

if (app.Environment.IsDevelopment())
{
    // Maps the OpenAPI JSON endpoint
    app.MapOpenApi();

    // Scalar UI — loads at /scalar/v1
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