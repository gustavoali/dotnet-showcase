using FluentAssertions;
using Moq;
using TaskManager.Application.Common.Interfaces;
using TaskManager.Application.Common.Models;
using TaskManager.Application.Features.Auth.Commands.Register;

namespace TaskManager.Application.Tests.Features.Auth.Commands.Register;

public class RegisterCommandHandlerTests
{
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly RegisterCommandHandler _handler;

    public RegisterCommandHandlerTests()
    {
        _authServiceMock = new Mock<IAuthService>();
        _handler = new RegisterCommandHandler(_authServiceMock.Object);
    }

    [Fact]
    public async Task Handle_Should_DelegateToAuthService()
    {
        // Arrange
        var expectedResult = AuthResult.Success("jwt-token", Guid.NewGuid().ToString());
        _authServiceMock
            .Setup(s => s.RegisterAsync(
                "test@example.com", "Password1", "John", "Doe",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var command = new RegisterCommand("test@example.com", "Password1", "John", "Doe");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        result.Succeeded.Should().BeTrue();
        result.Token.Should().Be("jwt-token");
        _authServiceMock.Verify(
            s => s.RegisterAsync(
                "test@example.com", "Password1", "John", "Doe",
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenEmailAlreadyExists()
    {
        // Arrange
        var expectedResult = AuthResult.Failure("A user with this email address already exists.");
        _authServiceMock
            .Setup(s => s.RegisterAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var command = new RegisterCommand("existing@example.com", "Password1", "John", "Doe");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("already exists"));
    }
}
