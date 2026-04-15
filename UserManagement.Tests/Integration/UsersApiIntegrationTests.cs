using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Text;
using System.Text.Json;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Enums;
using UserManagement.Infrastructure.Data;
using Xunit;

namespace UserManagement.Tests.Integration
{
    /// <summary>
    /// Custom factory that boots the API with an in-memory database.
    /// Uses a fixed database name so all tests share the same seeded data.
    /// SQL Server is skipped because DependencyInjection.cs checks
    /// for the Testing environment before registering it.
    /// </summary>
    public class TestWebApplicationFactory : WebApplicationFactory<Program>
    {
        // Fixed name ensures all tests in this class share
        // the same in-memory database instance and seed data
        private const string TestDbName = "IntegrationTestDb";

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                // Register in-memory DB with a fixed name
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseInMemoryDatabase(TestDbName));

                // Seed test data
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider
                              .GetRequiredService<ApplicationDbContext>();

                db.Database.EnsureCreated();
                SeedDatabase(db);
            });
        }

        private static void SeedDatabase(ApplicationDbContext db)
        {
            // Guard prevents duplicate seeding if called multiple times
            if (db.Users.Any()) return;

            db.Groups.Add(new Group
            {
                Name = "Testers",
                Description = "QA team",
                Permissions = new List<Permission>
            {
                new Permission
                {
                    Name  = "Test Access",
                    Level = PermissionLevel.Level1
                }
            }
            });

            db.Users.AddRange(
                new User
                {
                    FirstName = "Test",
                    LastName = "User",
                    Email = "test@test.com",
                    IsActive = true
                },
                new User
                {
                    FirstName = "Inactive",
                    LastName = "User",
                    Email = "inactive@test.com",
                    IsActive = false
                }
            );

            db.SaveChanges();
        }
    }


    /// <summary>
    /// Integration tests using TestWebApplicationFactory.
    /// Tests the full HTTP pipeline without a real SQL Server.
    /// </summary>
    public class UsersApiIntegrationTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly HttpClient _client;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public UsersApiIntegrationTests(TestWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetUsers_ShouldReturn200_WithUserList()
        {
            var response = await _client.GetAsync("/api/users");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetUserById_ShouldReturn404_WhenUserDoesNotExist()
        {
            var response = await _client.GetAsync("/api/users/99999");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetUserCount_ShouldReturn200_WithCount()
        {
            var response = await _client.GetAsync("/api/users/count");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(json, _jsonOptions);
            result.GetProperty("totalActiveUsers").GetInt32()
                  .Should().BeGreaterThanOrEqualTo(0);
        }

        [Fact]
        public async Task GetCountPerGroup_ShouldReturn200()
        {
            var response = await _client.GetAsync("/api/users/count-per-group");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task CreateUser_ShouldReturn201_WhenValid()
        {
            var payload = new
            {
                firstName = "Integration",
                lastName = "Test",
                email = $"integration_{Guid.NewGuid()}@test.com"
            };

            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("/api/users", content);
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [Fact]
        public async Task CreateUser_ShouldReturn409_WhenEmailAlreadyExists()
        {
            var payload = new
            {
                firstName = "Duplicate",
                lastName = "User",
                email = "test@test.com"
            };

            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("/api/users", content);
            response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }

        [Fact]
        public async Task CreateUser_ShouldReturn400_WhenEmailIsInvalid()
        {
            var payload = new
            {
                firstName = "Bad",
                lastName = "User",
                email = "not-a-valid-email"
            };

            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("/api/users", content);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task DeleteUser_ShouldReturn404_WhenUserDoesNotExist()
        {
            var response = await _client.DeleteAsync("/api/users/99999");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}