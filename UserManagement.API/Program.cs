using UserManagement.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Register Infrastructure services (DbContext + Repositories)
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();

app.Run();
