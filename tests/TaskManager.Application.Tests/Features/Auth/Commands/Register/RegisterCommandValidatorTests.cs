using FluentValidation.TestHelper;
using TaskManager.Application.Features.Auth.Commands.Register;

namespace TaskManager.Application.Tests.Features.Auth.Commands.Register;

/// <summary>
/// Unit tests for <see cref="RegisterCommandValidator"/>.
/// </summary>
public class RegisterCommandValidatorTests
{
    private readonly RegisterCommandValidator _validator = new();

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    [InlineData("missing@")]
    public void Should_HaveError_WhenEmailIsInvalid(string email)
    {
        // Arrange
        var command = new RegisterCommand(email, "Password1", "John", "Doe");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("")]
    [InlineData("Short1")]
    [InlineData("nouppercase1")]
    [InlineData("NoDigitHere")]
    public void Should_HaveError_WhenPasswordIsInvalid(string password)
    {
        // Arrange
        var command = new RegisterCommand("test@example.com", password, "John", "Doe");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Should_HaveError_WhenFirstNameIsEmpty()
    {
        // Arrange
        var command = new RegisterCommand("test@example.com", "Password1", "", "Doe");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [Fact]
    public void Should_HaveError_WhenLastNameIsEmpty()
    {
        // Arrange
        var command = new RegisterCommand("test@example.com", "Password1", "John", "");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LastName);
    }

    [Fact]
    public void Should_NotHaveError_WhenCommandIsValid()
    {
        // Arrange
        var command = new RegisterCommand("test@example.com", "StrongPass1", "John", "Doe");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
