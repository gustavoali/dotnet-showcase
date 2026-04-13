using FluentAssertions;
using TaskManager.Application.Common.Exceptions;

namespace TaskManager.Application.Tests.Common.Exceptions;

public class NotFoundExceptionTests
{
    [Fact]
    public void Constructor_Default_Should_HaveDefaultMessage()
    {
        // Act
        var exception = new NotFoundException();

        // Assert
        exception.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Constructor_WithNameAndKey_Should_FormatMessageCorrectly()
    {
        // Arrange
        var key = Guid.NewGuid();

        // Act
        var exception = new NotFoundException("Project", key);

        // Assert
        exception.Message.Should().Be($"Entity \"Project\" ({key}) was not found.");
    }

    [Fact]
    public void Constructor_WithMessage_Should_SetMessage()
    {
        // Act
        var exception = new NotFoundException("Custom message");

        // Assert
        exception.Message.Should().Be("Custom message");
    }

    [Fact]
    public void Constructor_WithMessageAndInnerException_Should_SetBoth()
    {
        // Arrange
        var inner = new InvalidOperationException("inner");

        // Act
        var exception = new NotFoundException("outer message", inner);

        // Assert
        exception.Message.Should().Be("outer message");
        exception.InnerException.Should().Be(inner);
    }

    [Theory]
    [InlineData("User", "123")]
    [InlineData("TaskItem", "abc-def")]
    public void Constructor_WithNameAndKey_Should_IncludeEntityNameAndKey(string name, string key)
    {
        // Act
        var exception = new NotFoundException(name, key);

        // Assert
        exception.Message.Should().Contain(name);
        exception.Message.Should().Contain(key);
    }
}
