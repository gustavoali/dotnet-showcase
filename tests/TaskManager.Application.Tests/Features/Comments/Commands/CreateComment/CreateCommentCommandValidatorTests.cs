using FluentValidation.TestHelper;
using TaskManager.Application.Features.Comments.Commands.CreateComment;

namespace TaskManager.Application.Tests.Features.Comments.Commands.CreateComment;

public class CreateCommentCommandValidatorTests
{
    private readonly CreateCommentCommandValidator _validator = new();

    [Fact]
    public void Should_HaveError_WhenTaskItemIdIsEmpty()
    {
        // Arrange
        var command = new CreateCommentCommand(Guid.Empty, "Valid content");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TaskItemId);
    }

    [Fact]
    public void Should_HaveError_WhenContentIsEmpty()
    {
        // Arrange
        var command = new CreateCommentCommand(Guid.NewGuid(), "");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Content);
    }

    [Fact]
    public void Should_HaveError_WhenContentExceeds5000Characters()
    {
        // Arrange
        var command = new CreateCommentCommand(Guid.NewGuid(), new string('A', 5001));

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Content);
    }

    [Fact]
    public void Should_NotHaveError_WhenCommandIsValid()
    {
        // Arrange
        var command = new CreateCommentCommand(Guid.NewGuid(), "This is a valid comment.");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_NotHaveError_WhenContentIsExactly5000Characters()
    {
        // Arrange
        var command = new CreateCommentCommand(Guid.NewGuid(), new string('A', 5000));

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
