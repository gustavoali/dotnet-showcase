using FluentValidation.TestHelper;
using TaskManager.Application.Features.Auth.Commands.Login;

namespace TaskManager.Application.Tests.Features.Auth.Commands.Login;

public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator = new();

    [Fact]
    public void Should_HaveError_WhenEmailIsEmpty()
    {
        // Arrange
        var command = new LoginCommand("", "Password1");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("missing@")]
    [InlineData("@missing-local.com")]
    public void Should_HaveError_WhenEmailIsInvalid(string email)
    {
        // Arrange
        var command = new LoginCommand(email, "Password1");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Should_HaveError_WhenPasswordIsEmpty()
    {
        // Arrange
        var command = new LoginCommand("test@example.com", "");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Should_NotHaveError_WhenCommandIsValid()
    {
        // Arrange
        var command = new LoginCommand("test@example.com", "anypassword");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
