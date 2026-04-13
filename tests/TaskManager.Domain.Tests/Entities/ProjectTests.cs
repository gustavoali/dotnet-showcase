using FluentAssertions;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;

namespace TaskManager.Domain.Tests.Entities;

/// <summary>
/// Unit tests for the <see cref="Project"/> entity.
/// </summary>
public class ProjectTests
{
    [Fact]
    public void Constructor_Should_SetDefaultValues()
    {
        // Act
        var project = new Project();

        // Assert
        project.Id.Should().Be(Guid.Empty);
        project.Name.Should().BeEmpty();
        project.Description.Should().BeEmpty();
        project.Status.Should().Be(ProjectStatus.Active);
        project.Tasks.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void Properties_Should_BeSettable()
    {
        // Arrange
        var id = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        // Act
        var project = new Project
        {
            Id = id,
            Name = "Test Project",
            Description = "A test project",
            OwnerId = ownerId,
            Status = ProjectStatus.Completed,
            CreatedAt = now,
            UpdatedAt = now
        };

        // Assert
        project.Id.Should().Be(id);
        project.Name.Should().Be("Test Project");
        project.Description.Should().Be("A test project");
        project.OwnerId.Should().Be(ownerId);
        project.Status.Should().Be(ProjectStatus.Completed);
        project.CreatedAt.Should().Be(now);
        project.UpdatedAt.Should().Be(now);
    }

    [Fact]
    public void Tasks_Collection_Should_AllowAdding()
    {
        // Arrange
        var project = new Project();
        var task = new TaskItem { Id = Guid.NewGuid(), Title = "Task 1" };

        // Act
        project.Tasks.Add(task);

        // Assert
        project.Tasks.Should().HaveCount(1);
        project.Tasks.Should().Contain(task);
    }
}
