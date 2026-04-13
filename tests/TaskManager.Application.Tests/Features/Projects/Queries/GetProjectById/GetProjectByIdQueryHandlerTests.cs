using FluentAssertions;
using Moq;
using TaskManager.Application.Common.Exceptions;
using TaskManager.Application.Common.Mappings;
using TaskManager.Application.Features.Projects.Queries.GetProjectById;
using TaskManager.Application.Tests.Helpers;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.Tests.Features.Projects.Queries.GetProjectById;

public class GetProjectByIdQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IRepository<Project>> _projectRepoMock;
    private readonly GetProjectByIdQueryHandler _handler;

    public GetProjectByIdQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _projectRepoMock = new Mock<IRepository<Project>>();
        _unitOfWorkMock.Setup(u => u.Projects).Returns(_projectRepoMock.Object);

        MappingConfig.RegisterMappings();

        _handler = new GetProjectByIdQueryHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_Should_ReturnProjectDto_WhenProjectExists()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var project = new Project
        {
            Id = projectId,
            Name = "Test Project",
            Description = "Test Description",
            OwnerId = ownerId,
            Owner = new User { Id = ownerId, FirstName = "John", LastName = "Doe" },
            Tasks = new List<TaskItem> { new() { Id = Guid.NewGuid(), Title = "Task 1" } },
            CreatedAt = DateTime.UtcNow
        };

        var queryable = new List<Project> { project }.AsAsyncQueryable();
        _projectRepoMock.Setup(r => r.Query()).Returns(queryable);

        var query = new GetProjectByIdQuery(projectId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(projectId);
        result.Name.Should().Be("Test Project");
        result.Description.Should().Be("Test Description");
        result.OwnerId.Should().Be(ownerId);
        result.OwnerName.Should().Be("John Doe");
        result.TaskCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_Should_ThrowNotFoundException_WhenProjectNotFound()
    {
        // Arrange
        var queryable = new List<Project>().AsAsyncQueryable();
        _projectRepoMock.Setup(r => r.Query()).Returns(queryable);

        var query = new GetProjectByIdQuery(Guid.NewGuid());

        // Act
        var act = () => _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_Should_ReturnZeroTaskCount_WhenProjectHasNoTasks()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var project = new Project
        {
            Id = projectId,
            Name = "Empty Project",
            Description = "No tasks",
            OwnerId = ownerId,
            Owner = new User { Id = ownerId, FirstName = "Jane", LastName = "Smith" },
            Tasks = new List<TaskItem>(),
            CreatedAt = DateTime.UtcNow
        };

        var queryable = new List<Project> { project }.AsAsyncQueryable();
        _projectRepoMock.Setup(r => r.Query()).Returns(queryable);

        var query = new GetProjectByIdQuery(projectId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.TaskCount.Should().Be(0);
    }
}
