using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Enums;
using UserManagement.Infrastructure.Data;
using UserManagement.Infrastructure.Repositories;
using Xunit;

namespace UserManagement.Tests.Unit.Repositories
{
    /// <summary>
    /// Unit tests for UserRepository using an in-memory database.
    /// Each test gets a fresh database via a unique name to prevent
    /// state leaking between tests.
    /// </summary>
    public class UserRepositoryTests
    {
        // ─────────────────────────────────────────
        // Helpers
        // ─────────────────────────────────────────
        private ApplicationDbContext CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            return new ApplicationDbContext(options);
        }

        private async Task SeedTestDataAsync(ApplicationDbContext context)
        {
            var group = new Group
            {
                Name = "Administrators",
                Description = "Admin group",
                Permissions = new List<Permission>
                {
                    new Permission
                    {
                        Name  = "Full Access",
                        Level = PermissionLevel.Level5
                    }
                }
            };

            var users = new List<User>
            {
                new User
                {
                    FirstName = "Alice",
                    LastName  = "Smith",
                    Email     = "alice@test.com",
                    IsActive  = true
                },
                new User
                {
                    FirstName = "Bob",
                    LastName  = "Jones",
                    Email     = "bob@test.com",
                    IsActive  = true
                },
                new User
                {
                    FirstName = "Carol",
                    LastName  = "White",
                    Email     = "carol@test.com",
                    IsActive  = false
                }
            };

            await context.Groups.AddAsync(group);
            await context.Users.AddRangeAsync(users);
            await context.SaveChangesAsync();
        }

        // ─────────────────────────────────────────
        // GetAllAsync Tests
        // ─────────────────────────────────────────
        [Fact]
        public async Task GetAllAsync_ShouldReturnAllUsers_WhenUsersExist()
        {
            await using var context = CreateContext(
                nameof(GetAllAsync_ShouldReturnAllUsers_WhenUsersExist));
            await SeedTestDataAsync(context);
            var repository = new UserRepository(context);

            var result = await repository.GetAllAsync();

            result.Should().HaveCount(3);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoUsersExist()
        {
            await using var context = CreateContext(
                nameof(GetAllAsync_ShouldReturnEmptyList_WhenNoUsersExist));
            var repository = new UserRepository(context);

            var result = await repository.GetAllAsync();

            result.Should().BeEmpty();
        }

        // ─────────────────────────────────────────
        // GetByIdAsync Tests
        // ─────────────────────────────────────────
        [Fact]
        public async Task GetByIdAsync_ShouldReturnUser_WhenUserExists()
        {
            await using var context = CreateContext(
                nameof(GetByIdAsync_ShouldReturnUser_WhenUserExists));
            await SeedTestDataAsync(context);
            var repository = new UserRepository(context);
            var existingUser = context.Users.First();

            var result = await repository.GetByIdAsync(existingUser.Id);

            result.Should().NotBeNull();
            result!.Email.Should().Be(existingUser.Email);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenUserDoesNotExist()
        {
            await using var context = CreateContext(
                nameof(GetByIdAsync_ShouldReturnNull_WhenUserDoesNotExist));
            var repository = new UserRepository(context);

            var result = await repository.GetByIdAsync(999);

            result.Should().BeNull();
        }

        // ─────────────────────────────────────────
        // AddAsync Tests
        // ─────────────────────────────────────────
        [Fact]
        public async Task AddAsync_ShouldPersistUser_WhenValidUserProvided()
        {
            await using var context = CreateContext(
                nameof(AddAsync_ShouldPersistUser_WhenValidUserProvided));
            var repository = new UserRepository(context);

            var newUser = new User
            {
                FirstName = "David",
                LastName = "Brown",
                Email = "david@test.com",
                IsActive = true
            };

            await repository.AddAsync(newUser);

            var saved = await context.Users
                .FirstOrDefaultAsync(u => u.Email == "david@test.com");
            saved.Should().NotBeNull();
            saved!.FirstName.Should().Be("David");
        }

        // ─────────────────────────────────────────
        // UpdateAsync Tests
        // ─────────────────────────────────────────
        [Fact]
        public async Task UpdateAsync_ShouldModifyUser_WhenUserExists()
        {
            await using var context = CreateContext(
                nameof(UpdateAsync_ShouldModifyUser_WhenUserExists));
            await SeedTestDataAsync(context);
            var repository = new UserRepository(context);

            var user = await context.Users.FirstAsync();
            user.FirstName = "UpdatedName";

            await repository.UpdateAsync(user);

            var updated = await context.Users.FindAsync(user.Id);
            updated!.FirstName.Should().Be("UpdatedName");
        }

        // ─────────────────────────────────────────
        // DeleteAsync Tests
        // ─────────────────────────────────────────
        [Fact]
        public async Task DeleteAsync_ShouldRemoveUser_WhenUserExists()
        {
            await using var context = CreateContext(
                nameof(DeleteAsync_ShouldRemoveUser_WhenUserExists));
            await SeedTestDataAsync(context);
            var repository = new UserRepository(context);

            var user = await context.Users.FirstAsync();
            var userId = user.Id;

            await repository.DeleteAsync(userId);

            var deleted = await context.Users.FindAsync(userId);
            deleted.Should().BeNull();
        }

        [Fact]
        public async Task DeleteAsync_ShouldNotThrow_WhenUserDoesNotExist()
        {
            await using var context = CreateContext(
                nameof(DeleteAsync_ShouldNotThrow_WhenUserDoesNotExist));
            var repository = new UserRepository(context);

            var act = async () => await repository.DeleteAsync(999);

            await act.Should().NotThrowAsync();
        }

        // ─────────────────────────────────────────
        // GetByEmailAsync Tests
        // ─────────────────────────────────────────
        [Fact]
        public async Task GetByEmailAsync_ShouldReturnUser_WhenEmailExists()
        {
            await using var context = CreateContext(
                nameof(GetByEmailAsync_ShouldReturnUser_WhenEmailExists));
            await SeedTestDataAsync(context);
            var repository = new UserRepository(context);

            var result = await repository.GetByEmailAsync("alice@test.com");

            result.Should().NotBeNull();
            result!.FirstName.Should().Be("Alice");
        }

        [Fact]
        public async Task GetByEmailAsync_ShouldBeCaseInsensitive()
        {
            await using var context = CreateContext(
                nameof(GetByEmailAsync_ShouldBeCaseInsensitive));
            await SeedTestDataAsync(context);
            var repository = new UserRepository(context);

            var result = await repository.GetByEmailAsync("ALICE@TEST.COM");

            result.Should().NotBeNull();
            result!.Email.Should().Be("alice@test.com");
        }

        [Fact]
        public async Task GetByEmailAsync_ShouldReturnNull_WhenEmailDoesNotExist()
        {
            await using var context = CreateContext(
                nameof(GetByEmailAsync_ShouldReturnNull_WhenEmailDoesNotExist));
            await SeedTestDataAsync(context);
            var repository = new UserRepository(context);

            var result = await repository.GetByEmailAsync("nobody@test.com");

            result.Should().BeNull();
        }

        // ─────────────────────────────────────────
        // GetTotalUserCountAsync Tests
        // ─────────────────────────────────────────
        [Fact]
        public async Task GetTotalUserCountAsync_ShouldReturnOnlyActiveUsers()
        {
            // Seeds 2 active + 1 inactive — expects count of 2
            await using var context = CreateContext(
                nameof(GetTotalUserCountAsync_ShouldReturnOnlyActiveUsers));
            await SeedTestDataAsync(context);
            var repository = new UserRepository(context);

            var count = await repository.GetTotalUserCountAsync();

            count.Should().Be(2);
        }

        // ─────────────────────────────────────────
        // ExistsAsync Tests
        // ─────────────────────────────────────────
        [Fact]
        public async Task ExistsAsync_ShouldReturnTrue_WhenUserExists()
        {
            await using var context = CreateContext(
                nameof(ExistsAsync_ShouldReturnTrue_WhenUserExists));
            await SeedTestDataAsync(context);
            var repository = new UserRepository(context);
            var user = await context.Users.FirstAsync();

            var exists = await repository.ExistsAsync(user.Id);

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsAsync_ShouldReturnFalse_WhenUserDoesNotExist()
        {
            await using var context = CreateContext(
                nameof(ExistsAsync_ShouldReturnFalse_WhenUserDoesNotExist));
            var repository = new UserRepository(context);

            var exists = await repository.ExistsAsync(999);

            exists.Should().BeFalse();
        }
    }
}