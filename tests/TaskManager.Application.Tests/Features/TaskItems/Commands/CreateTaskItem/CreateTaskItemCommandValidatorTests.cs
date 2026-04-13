using FluentValidation.TestHelper;
using TaskManager.Application.Features.TaskItems.Commands.CreateTaskItem;
using TaskManager.Domain.Enums;

namespace TaskManager.Application.Tests.Features.TaskItems.Commands.CreateTaskItem;

/// <summary>
/// Unit tests for <see cref="CreateTaskItemCommandValidator"/>.
/// </summary>
public class CreateTaskItemCommandValidatorTests
{
    private readonly CreateTaskItemCommandValidator _validator = new();

    [Fact]
    public void Should_HaveError_WhenTitleIsEmpty()
    {
        // Arrange
        var command = new CreateTaskItemCommand("", "Desc", Guid.NewGuid(), TaskPriority.Medium, null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void Should_HaveError_WhenProjectIdIsEmpty()
    {
        // Arrange
        var command = new CreateTaskItemCommand("Title", "Desc", Guid.Empty, TaskPriority.Medium, null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ProjectId);
    }

    [Fact]
    public void Should_NotHaveError_WhenCommandIsValid()
    {
        // Arrange
        var command = new CreateTaskItemCommand("Valid Title", "Valid Desc", Guid.NewGuid(), TaskPriority.High, DateTime.UtcNow.AddDays(7));

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
