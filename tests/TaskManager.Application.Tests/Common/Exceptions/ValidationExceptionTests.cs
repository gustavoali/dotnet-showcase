using FluentAssertions;
using FluentValidation.Results;
using ValidationException = TaskManager.Application.Common.Exceptions.ValidationException;

namespace TaskManager.Application.Tests.Common.Exceptions;

public class ValidationExceptionTests
{
    [Fact]
    public void Constructor_Default_Should_HaveEmptyErrors()
    {
        // Act
        var exception = new ValidationException();

        // Assert
        exception.Errors.Should().NotBeNull();
        exception.Errors.Should().BeEmpty();
        exception.Message.Should().Be("One or more validation failures have occurred.");
    }

    [Fact]
    public void Constructor_WithFailures_Should_GroupByPropertyName()
    {
        // Arrange
        var failures = new List<ValidationFailure>
        {
            new("Name", "Name is required."),
            new("Name", "Name must not exceed 200 characters."),
            new("Email", "Email is required.")
        };

        // Act
        var exception = new ValidationException(failures);

        // Assert
        exception.Errors.Should().HaveCount(2);
        exception.Errors.Should().ContainKey("Name");
        exception.Errors.Should().ContainKey("Email");
        exception.Errors["Name"].Should().HaveCount(2);
        exception.Errors["Name"].Should().Contain("Name is required.");
        exception.Errors["Name"].Should().Contain("Name must not exceed 200 characters.");
        exception.Errors["Email"].Should().HaveCount(1);
        exception.Errors["Email"].Should().Contain("Email is required.");
    }

    [Fact]
    public void Constructor_WithSingleFailure_Should_CreateSingleEntry()
    {
        // Arrange
        var failures = new List<ValidationFailure>
        {
            new("Password", "Password is too short.")
        };

        // Act
        var exception = new ValidationException(failures);

        // Assert
        exception.Errors.Should().HaveCount(1);
        exception.Errors["Password"].Should().ContainSingle()
            .Which.Should().Be("Password is too short.");
    }

    [Fact]
    public void Constructor_WithEmptyFailures_Should_HaveEmptyErrors()
    {
        // Arrange
        var failures = new List<ValidationFailure>();

        // Act
        var exception = new ValidationException(failures);

        // Assert
        exception.Errors.Should().BeEmpty();
    }
}
