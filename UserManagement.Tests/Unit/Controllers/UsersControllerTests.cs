using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using UserManagement.API.Controllers;
using UserManagement.API.DTOs.User;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Interfaces;
using Xunit;

namespace UserManagement.Tests.Unit.Controllers
{
    /// <summary>
    /// Unit tests for the API UsersController.
    /// IUserRepository is mocked — no database needed.
    /// Tests focus purely on controller logic and HTTP responses.
    /// </summary>
    public class UsersControllerTests
    {
        private readonly Mock<IUserRepository> _mockRepo;
        private readonly Mock<ILogger<UsersController>> _mockLogger;
        private readonly UsersController _controller;

        public UsersControllerTests()
        {
            _mockRepo = new Mock<IUserRepository>();
            _mockLogger = new Mock<ILogger<UsersController>>();
            _controller = new UsersController(_mockRepo.Object, _mockLogger.Object);
        }

        // ─────────────────────────────────────────
        // GET api/users
        // ─────────────────────────────────────────
        [Fact]
        public async Task GetAll_ShouldReturn200_WithListOfUsers()
        {
            var users = new List<User>
            {
                new User { Id = 1, FirstName = "Alice", LastName = "Smith",
                           Email = "alice@test.com", IsActive = true },
                new User { Id = 2, FirstName = "Bob",   LastName = "Jones",
                           Email = "bob@test.com",   IsActive = true }
            };

            _mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(users);

            var result = await _controller.GetAll();

            result.Should().BeOfType<OkObjectResult>()
                  .Which.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task GetAll_ShouldReturn500_WhenRepositoryThrows()
        {
            _mockRepo.Setup(r => r.GetAllAsync())
                     .ThrowsAsync(new Exception("DB error"));

            var result = await _controller.GetAll();

            result.Should().BeOfType<ObjectResult>()
                  .Which.StatusCode.Should().Be(500);
        }

        // ─────────────────────────────────────────
        // GET api/users/{id}
        // ─────────────────────────────────────────
        [Fact]
        public async Task GetById_ShouldReturn200_WhenUserExists()
        {
            var user = new User
            {
                Id = 1,
                FirstName = "Alice",
                LastName = "Smith",
                Email = "alice@test.com",
                IsActive = true
            };

            _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);

            var result = await _controller.GetById(1);

            result.Should().BeOfType<OkObjectResult>()
                  .Which.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task GetById_ShouldReturn404_WhenUserDoesNotExist()
        {
            _mockRepo.Setup(r => r.GetByIdAsync(999))
                     .ReturnsAsync((User?)null);

            var result = await _controller.GetById(999);

            result.Should().BeOfType<NotFoundObjectResult>()
                  .Which.StatusCode.Should().Be(404);
        }

        // ─────────────────────────────────────────
        // POST api/users
        // ─────────────────────────────────────────
        [Fact]
        public async Task Create_ShouldReturn201_WhenUserIsValid()
        {
            var dto = new CreateUserDto
            {
                FirstName = "Alice",
                LastName = "Smith",
                Email = "alice@test.com"
            };

            _mockRepo.Setup(r => r.GetByEmailAsync(dto.Email))
                     .ReturnsAsync((User?)null);

            _mockRepo.Setup(r => r.AddAsync(It.IsAny<User>()))
                     .Returns(Task.CompletedTask);

            var result = await _controller.Create(dto);

            result.Should().BeOfType<CreatedAtActionResult>()
                  .Which.StatusCode.Should().Be(201);
        }

        [Fact]
        public async Task Create_ShouldReturn409_WhenEmailAlreadyExists()
        {
            var dto = new CreateUserDto
            {
                FirstName = "Alice",
                LastName = "Smith",
                Email = "alice@test.com"
            };

            _mockRepo.Setup(r => r.GetByEmailAsync(dto.Email))
                     .ReturnsAsync(new User { Email = dto.Email });

            var result = await _controller.Create(dto);

            result.Should().BeOfType<ConflictObjectResult>()
                  .Which.StatusCode.Should().Be(409);
        }

        // ─────────────────────────────────────────
        // PUT api/users/{id}
        // ─────────────────────────────────────────
        [Fact]
        public async Task Update_ShouldReturn200_WhenUserExists()
        {
            var existingUser = new User
            {
                Id = 1,
                FirstName = "Alice",
                LastName = "Smith",
                Email = "alice@test.com",
                IsActive = true
            };

            var dto = new UpdateUserDto
            {
                FirstName = "AliceUpdated",
                LastName = "Smith",
                Email = "alice@test.com",
                IsActive = true
            };

            _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingUser);
            _mockRepo.Setup(r => r.UpdateAsync(It.IsAny<User>()))
                     .Returns(Task.CompletedTask);

            var result = await _controller.Update(1, dto);

            result.Should().BeOfType<OkObjectResult>()
                  .Which.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task Update_ShouldReturn404_WhenUserDoesNotExist()
        {
            _mockRepo.Setup(r => r.GetByIdAsync(999))
                     .ReturnsAsync((User?)null);

            var dto = new UpdateUserDto
            {
                FirstName = "Alice",
                LastName = "Smith",
                Email = "alice@test.com",
                IsActive = true
            };

            var result = await _controller.Update(999, dto);

            result.Should().BeOfType<NotFoundObjectResult>()
                  .Which.StatusCode.Should().Be(404);
        }

        // ─────────────────────────────────────────
        // DELETE api/users/{id}
        // ─────────────────────────────────────────
        [Fact]
        public async Task Delete_ShouldReturn204_WhenUserExists()
        {
            _mockRepo.Setup(r => r.ExistsAsync(1)).ReturnsAsync(true);
            _mockRepo.Setup(r => r.DeleteAsync(1)).Returns(Task.CompletedTask);

            var result = await _controller.Delete(1);

            result.Should().BeOfType<NoContentResult>()
                  .Which.StatusCode.Should().Be(204);
        }

        [Fact]
        public async Task Delete_ShouldReturn404_WhenUserDoesNotExist()
        {
            _mockRepo.Setup(r => r.ExistsAsync(999)).ReturnsAsync(false);

            var result = await _controller.Delete(999);

            result.Should().BeOfType<NotFoundObjectResult>()
                  .Which.StatusCode.Should().Be(404);
        }

        // ─────────────────────────────────────────
        // GET api/users/count
        // ─────────────────────────────────────────
        [Fact]
        public async Task GetUserCount_ShouldReturn200_WithCorrectCount()
        {
            _mockRepo.Setup(r => r.GetTotalUserCountAsync()).ReturnsAsync(42);

            var result = await _controller.GetUserCount();

            result.Should().BeOfType<OkObjectResult>()
                  .Which.StatusCode.Should().Be(200);
        }
    }
}