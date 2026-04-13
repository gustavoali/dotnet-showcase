using FluentAssertions;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;

namespace TaskManager.Domain.Tests.Entities;

/// <summary>
/// Unit tests for the <see cref="TaskItem"/> entity.
/// </summary>
public class TaskItemTests
{
    [Fact]
    public void Constructor_Should_SetDefaultValues()
    {
        // Act
        var taskItem = new TaskItem();

        // Assert
        taskItem.Id.Should().Be(Guid.Empty);
        taskItem.Title.Should().BeEmpty();
        taskItem.Description.Should().BeEmpty();
        taskItem.Status.Should().Be(TaskItemStatus.Todo);
        taskItem.Priority.Should().Be(TaskPriority.Medium);
        taskItem.DueDate.Should().BeNull();
        taskItem.AssigneeId.Should().BeNull();
        taskItem.Comments.Should().NotBeNull().And.BeEmpty();
        taskItem.Tags.Should().NotBeNull().And.BeEmpty();
    }

    [Theory]
    [InlineData(TaskItemStatus.Todo)]
    [InlineData(TaskItemStatus.InProgress)]
    [InlineData(TaskItemStatus.Done)]
    [InlineData(TaskItemStatus.Cancelled)]
    public void Status_Should_AcceptAllValidValues(TaskItemStatus status)
    {
        // Act
        var taskItem = new TaskItem { Status = status };

        // Assert
        taskItem.Status.Should().Be(status);
    }

    [Theory]
    [InlineData(TaskPriority.Low)]
    [InlineData(TaskPriority.Medium)]
    [InlineData(TaskPriority.High)]
    [InlineData(TaskPriority.Critical)]
    public void Priority_Should_AcceptAllValidValues(TaskPriority priority)
    {
        // Act
        var taskItem = new TaskItem { Priority = priority };

        // Assert
        taskItem.Priority.Should().Be(priority);
    }

    [Fact]
    public void Comments_And_Tags_Should_AllowAdding()
    {
        // Arrange
        var taskItem = new TaskItem();
        var comment = new Comment { Id = Guid.NewGuid(), Content = "Test" };
        var tag = new Tag { Id = Guid.NewGuid(), Name = "Bug" };

        // Act
        taskItem.Comments.Add(comment);
        taskItem.Tags.Add(tag);

        // Assert
        taskItem.Comments.Should().HaveCount(1);
        taskItem.Tags.Should().HaveCount(1);
    }
}
