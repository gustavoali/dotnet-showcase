using FluentAssertions;
using Moq;
using TaskManager.Application.Common.Interfaces;
using TaskManager.Application.Common.Models;
using TaskManager.Application.Features.Auth.Commands.Login;

namespace TaskManager.Application.Tests.Features.Auth.Commands.Login;

public class LoginCommandHandlerTests
{
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _authServiceMock = new Mock<IAuthService>();
        _handler = new LoginCommandHandler(_authServiceMock.Object);
    }

    [Fact]
    public async Task Handle_Should_DelegateToAuthService()
    {
        // Arrange
        var expectedResult = AuthResult.Success("jwt-token", Guid.NewGuid().ToString());
        _authServiceMock
            .Setup(s => s.LoginAsync("test@example.com", "Password1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var command = new LoginCommand("test@example.com", "Password1");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        _authServiceMock.Verify(
            s => s.LoginAsync("test@example.com", "Password1", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenAuthServiceFails()
    {
        // Arrange
        var expectedResult = AuthResult.Failure("Invalid email or password.");
        _authServiceMock
            .Setup(s => s.LoginAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var command = new LoginCommand("wrong@example.com", "wrong");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Errors.Should().Contain("Invalid email or password.");
    }
}
