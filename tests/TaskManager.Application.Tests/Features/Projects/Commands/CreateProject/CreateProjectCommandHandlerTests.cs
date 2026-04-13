using FluentAssertions;
using Moq;
using TaskManager.Application.Common.Interfaces;
using TaskManager.Application.Features.Projects.Commands.CreateProject;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.Tests.Features.Projects.Commands.CreateProject;

/// <summary>
/// Unit tests for <see cref="CreateProjectCommandHandler"/>.
/// </summary>
public class CreateProjectCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IRepository<Project>> _projectRepoMock;
    private readonly CreateProjectCommandHandler _handler;
    private readonly Guid _userId = Guid.NewGuid();

    public CreateProjectCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _projectRepoMock = new Mock<IRepository<Project>>();

        _unitOfWorkMock.Setup(u => u.Projects).Returns(_projectRepoMock.Object);
        _currentUserServiceMock.Setup(c => c.UserId).Returns(_userId);

        _handler = new CreateProjectCommandHandler(
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_Should_CallAddAsync_And_SaveChanges()
    {
        // Arrange
        var command = new CreateProjectCommand("My Project", "Description");
        Project? capturedProject = null;

        _projectRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Project>(), It.IsAny<CancellationToken>()))
            .Callback<Project, CancellationToken>((p, _) => capturedProject = p)
            .ReturnsAsync((Project p, CancellationToken _) => p);

        _projectRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) => capturedProject);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        _projectRepoMock.Verify(r => r.AddAsync(It.IsAny<Project>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_SetOwnerId_FromCurrentUser()
    {
        // Arrange
        var command = new CreateProjectCommand("My Project", "Description");
        Project? capturedProject = null;

        _projectRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Project>(), It.IsAny<CancellationToken>()))
            .Callback<Project, CancellationToken>((p, _) => capturedProject = p)
            .ReturnsAsync((Project p, CancellationToken _) => p);

        _projectRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) => capturedProject);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedProject.Should().NotBeNull();
        capturedProject!.OwnerId.Should().Be(_userId);
    }

    [Fact]
    public async Task Handle_Should_SetCreatedAt_ToUtcNow()
    {
        // Arrange
        var command = new CreateProjectCommand("My Project", "Description");
        var beforeUtc = DateTime.UtcNow;
        Project? capturedProject = null;

        _projectRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Project>(), It.IsAny<CancellationToken>()))
            .Callback<Project, CancellationToken>((p, _) => capturedProject = p)
            .ReturnsAsync((Project p, CancellationToken _) => p);

        _projectRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) => capturedProject);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);
        var afterUtc = DateTime.UtcNow;

        // Assert
        capturedProject.Should().NotBeNull();
        capturedProject!.CreatedAt.Should().BeOnOrAfter(beforeUtc).And.BeOnOrBefore(afterUtc);
    }
}
