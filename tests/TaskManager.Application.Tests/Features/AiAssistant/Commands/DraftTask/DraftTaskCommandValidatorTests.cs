using FluentAssertions;
using TaskManager.Application.Features.AiAssistant.Commands.DraftTask;

namespace TaskManager.Application.Tests.Features.AiAssistant.Commands.DraftTask;

/// <summary>
/// Unit tests for <see cref="DraftTaskCommandValidator"/>.
/// </summary>
public class DraftTaskCommandValidatorTests
{
    private readonly DraftTaskCommandValidator _validator = new();

    [Fact]
    public void Validate_Should_Pass_WhenInputIsValid()
    {
        // Arrange
        var command = new DraftTaskCommand("remember to pay suppliers on Friday");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_Should_Fail_WhenInputIsEmptyOrWhitespace(string input)
    {
        // Arrange
        var command = new DraftTaskCommand(input);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(DraftTaskCommand.Input));
    }

    [Fact]
    public void Validate_Should_Fail_WhenInputExceedsMaxLength()
    {
        // Arrange
        var command = new DraftTaskCommand(new string('x', DraftTaskCommandValidator.MaxInputLength + 1));

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(DraftTaskCommand.Input));
    }

    [Fact]
    public void Validate_Should_Pass_WhenInputIsAtMaxLength()
    {
        // Arrange
        var command = new DraftTaskCommand(new string('x', DraftTaskCommandValidator.MaxInputLength));

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
