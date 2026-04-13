using System.Linq.Expressions;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;
using TaskManager.Infrastructure.Services;

namespace TaskManager.Infrastructure.Tests.Services;

/// <summary>
/// Unit tests for <see cref="AuthService"/>.
/// </summary>
public class AuthServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IRepository<User>> _userRepoMock;
    private readonly IConfiguration _configuration;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _userRepoMock = new Mock<IRepository<User>>();
        _unitOfWorkMock.Setup(u => u.Users).Returns(_userRepoMock.Object);

        var configData = new Dictionary<string, string?>
        {
            { "Jwt:Secret", "ThisIsAVeryLongSecretKeyForTestingPurposesAtLeast32Bytes!" },
            { "Jwt:Issuer", "TestIssuer" },
            { "Jwt:Audience", "TestAudience" },
            { "Jwt:ExpirationInHours", "24" }
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        _authService = new AuthService(_unitOfWorkMock.Object, _configuration);
    }

    [Fact]
    public async Task RegisterAsync_Should_CreateUser_WhenEmailNotExists()
    {
        // Arrange
        _userRepoMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<User>());

        _userRepoMock
            .Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User u, CancellationToken _) => u);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _authService.RegisterAsync("new@example.com", "Password1", "John", "Doe");

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Token.Should().NotBeNullOrEmpty();
        result.UserId.Should().NotBeNullOrEmpty();
        _userRepoMock.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_Should_ReturnFailure_WhenEmailAlreadyExists()
    {
        // Arrange
        var existingUser = new User { Id = Guid.NewGuid(), Email = "existing@example.com" };
        _userRepoMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<User> { existingUser });

        // Act
        var result = await _authService.RegisterAsync("existing@example.com", "Password1", "John", "Doe");

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("already exists"));
        _userRepoMock.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_Should_ReturnSuccess_WhenCredentialsValid()
    {
        // Arrange
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("Password1");
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            PasswordHash = hashedPassword,
            FirstName = "John",
            LastName = "Doe"
        };

        _userRepoMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<User> { user });

        // Act
        var result = await _authService.LoginAsync("test@example.com", "Password1");

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Token.Should().NotBeNullOrEmpty();
        result.UserId.Should().Be(user.Id.ToString());
    }

    [Fact]
    public async Task LoginAsync_Should_ReturnFailure_WhenInvalidPassword()
    {
        // Arrange
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("CorrectPassword1");
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            PasswordHash = hashedPassword
        };

        _userRepoMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<User> { user });

        // Act
        var result = await _authService.LoginAsync("test@example.com", "WrongPassword1");

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task LoginAsync_Should_ReturnFailure_WhenUserNotFound()
    {
        // Arrange
        _userRepoMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<User>());

        // Act
        var result = await _authService.LoginAsync("nonexistent@example.com", "Password1");

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task RegisterAsync_Should_GenerateValidJwtToken()
    {
        // Arrange
        _userRepoMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<User>());

        _userRepoMock
            .Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User u, CancellationToken _) => u);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _authService.RegisterAsync("jwt@example.com", "Password1", "Jane", "Doe");

        // Assert
        result.Token.Should().NotBeNullOrEmpty();
        // JWT tokens have 3 parts separated by dots
        result.Token.Split('.').Should().HaveCount(3);
    }
}
