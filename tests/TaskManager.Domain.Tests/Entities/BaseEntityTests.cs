using FluentAssertions;
using TaskManager.Domain.Entities;

namespace TaskManager.Domain.Tests.Entities;

public class BaseEntityTests
{
    // Concrete derived class for testing the abstract BaseEntity
    private class TestEntity : BaseEntity { }

    [Fact]
    public void DefaultValues_Should_HaveExpectedDefaults()
    {
        // Act
        var entity = new TestEntity();

        // Assert
        entity.Id.Should().Be(Guid.Empty);
        entity.CreatedAt.Should().Be(default(DateTime));
        entity.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void Properties_Should_BeSettable()
    {
        // Arrange
        var id = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;
        var updatedAt = DateTime.UtcNow.AddHours(1);

        // Act
        var entity = new TestEntity
        {
            Id = id,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };

        // Assert
        entity.Id.Should().Be(id);
        entity.CreatedAt.Should().Be(createdAt);
        entity.UpdatedAt.Should().Be(updatedAt);
    }

    [Fact]
    public void UpdatedAt_Should_AcceptNull()
    {
        // Arrange
        var entity = new TestEntity { UpdatedAt = DateTime.UtcNow };

        // Act
        entity.UpdatedAt = null;

        // Assert
        entity.UpdatedAt.Should().BeNull();
    }
}
