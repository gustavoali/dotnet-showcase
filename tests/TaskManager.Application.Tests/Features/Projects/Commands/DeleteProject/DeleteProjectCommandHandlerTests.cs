using FluentAssertions;
using Moq;
using TaskManager.Application.Common.Exceptions;
using TaskManager.Application.Common.Interfaces;
using TaskManager.Application.Features.Projects.Commands.DeleteProject;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.Tests.Features.Projects.Commands.DeleteProject;

/// <summary>
/// Unit tests for <see cref="DeleteProjectCommandHandler"/>.
/// </summary>
public class DeleteProjectCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IRepository<Project>> _projectRepoMock;
    private readonly DeleteProjectCommandHandler _handler;
    private readonly Guid _userId = Guid.NewGuid();

    public DeleteProjectCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _projectRepoMock = new Mock<IRepository<Project>>();

        _unitOfWorkMock.Setup(u => u.Projects).Returns(_projectRepoMock.Object);
        _currentUserServiceMock.Setup(c => c.UserId).Returns(_userId);

        _handler = new DeleteProjectCommandHandler(
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_Should_DeleteProject_WhenOwner()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var project = new Project { Id = projectId, OwnerId = _userId };

        _projectRepoMock
            .Setup(r => r.GetByIdAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = new DeleteProjectCommand(projectId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _projectRepoMock.Verify(r => r.Delete(project), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_ThrowNotFoundException_WhenProjectNotFound()
    {
        // Arrange
        _projectRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Project?)null);

        var command = new DeleteProjectCommand(Guid.NewGuid());

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
        var project = new Project { Id = projectId, OwnerId = Guid.NewGuid() };

        _projectRepoMock
            .Setup(r => r.GetByIdAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        var command = new DeleteProjectCommand(projectId);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
}
