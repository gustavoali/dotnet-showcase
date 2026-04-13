using FluentAssertions;
using Moq;
using TaskManager.Application.Common.Exceptions;
using TaskManager.Application.Common.Interfaces;
using TaskManager.Application.Common.Mappings;
using TaskManager.Application.Features.Projects.Commands.UpdateProject;
using TaskManager.Application.Tests.Helpers;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.Tests.Features.Projects.Commands.UpdateProject;

public class UpdateProjectCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IRepository<Project>> _projectRepoMock;
    private readonly UpdateProjectCommandHandler _handler;
    private readonly Guid _userId = Guid.NewGuid();

    public UpdateProjectCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _projectRepoMock = new Mock<IRepository<Project>>();

        _unitOfWorkMock.Setup(u => u.Projects).Returns(_projectRepoMock.Object);
        _currentUserServiceMock.Setup(c => c.UserId).Returns(_userId);

        MappingConfig.RegisterMappings();

        _handler = new UpdateProjectCommandHandler(
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_Should_UpdateProject_WhenOwner()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var existingProject = new Project
        {
            Id = projectId,
            Name = "Old Name",
            Description = "Old Desc",
            OwnerId = _userId,
            Owner = new User { Id = _userId, FirstName = "John", LastName = "Doe" },
            Tasks = new List<TaskItem>()
        };

        var queryable = new List<Project> { existingProject }.AsAsyncQueryable();
        _projectRepoMock.Setup(r => r.Query()).Returns(queryable);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = new UpdateProjectCommand(projectId, "New Name", "New Desc", ProjectStatus.Completed);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        existingProject.Name.Should().Be("New Name");
        existingProject.Description.Should().Be("New Desc");
        existingProject.Status.Should().Be(ProjectStatus.Completed);
        _projectRepoMock.Verify(r => r.Update(existingProject), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_ThrowNotFoundException_WhenProjectNotFound()
    {
        // Arrange
        var queryable = new List<Project>().AsAsyncQueryable();
        _projectRepoMock.Setup(r => r.Query()).Returns(queryable);

        var command = new UpdateProjectCommand(Guid.NewGuid(), "Name", "Desc", ProjectStatus.Active);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_Should_ThrowForbiddenAccessException_WhenNotOwner()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var differentUserId = Guid.NewGuid();
        var existingProject = new Project
        {
            Id = projectId,
            Name = "Project",
            OwnerId = differentUserId,
            Owner = new User { Id = differentUserId, FirstName = "Other", LastName = "User" },
            Tasks = new List<TaskItem>()
        };

        var queryable = new List<Project> { existingProject }.AsAsyncQueryable();
        _projectRepoMock.Setup(r => r.Query()).Returns(queryable);

        var command = new UpdateProjectCommand(projectId, "Name", "Desc", ProjectStatus.Active);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
}
