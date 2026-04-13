using FluentAssertions;
using FluentValidation.TestHelper;
using TaskManager.Application.Features.Projects.Commands.CreateProject;

namespace TaskManager.Application.Tests.Features.Projects.Commands.CreateProject;

/// <summary>
/// Unit tests for <see cref="CreateProjectCommandValidator"/>.
/// </summary>
public class CreateProjectCommandValidatorTests
{
    private readonly CreateProjectCommandValidator _validator = new();

    [Fact]
    public void Should_HaveError_WhenNameIsEmpty()
    {
        // Arrange
        var command = new CreateProjectCommand("", "Valid description");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_HaveError_WhenNameExceeds200Characters()
    {
        // Arrange
        var command = new CreateProjectCommand(new string('A', 201), "Valid description");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_HaveError_WhenDescriptionExceeds2000Characters()
    {
        // Arrange
        var command = new CreateProjectCommand("Valid Name", new string('A', 2001));

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Should_NotHaveError_WhenCommandIsValid()
    {
        // Arrange
        var command = new CreateProjectCommand("Valid Name", "Valid description");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_NotHaveError_WhenDescriptionIsEmpty()
    {
        // Arrange - Description is optional
        var command = new CreateProjectCommand("Valid Name", "");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }
}
