using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using UserManagement.Domain.Interfaces;
using UserManagement.Infrastructure.Data;
using UserManagement.Infrastructure.Repositories;

namespace UserManagement.Infrastructure
{
    /// <summary>
    /// Extension method that registers all Infrastructure services.
    /// Skips SQL Server registration when running in Testing environment
    /// so integration tests can register their own in-memory database
    /// without getting a "two providers registered" conflict.
    /// </summary>
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration,
            IHostEnvironment? environment = null)
        {
            // Skip SQL Server during integration tests.
            // The test factory registers an in-memory DB instead.
            var isTesting = environment?.IsEnvironment("Testing") == true
                         || configuration["ASPNETCORE_ENVIRONMENT"] == "Testing";

            if (!isTesting)
            {
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer(
                        configuration.GetConnectionString("DefaultConnection"),
                        sqlOptions => sqlOptions.MigrationsAssembly(
                            typeof(ApplicationDbContext).Assembly.FullName)
                    )
                );
            }

            // Repositories always registered regardless of environment
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IGroupRepository, GroupRepository>();
            services.AddScoped<IPermissionRepository, PermissionRepository>();

            return services;
        }
    }
}