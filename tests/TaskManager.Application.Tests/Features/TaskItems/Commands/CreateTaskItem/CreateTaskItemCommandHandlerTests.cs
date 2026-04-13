using FluentAssertions;
using Moq;
using TaskManager.Application.Common.Exceptions;
using TaskManager.Application.Features.TaskItems.Commands.CreateTaskItem;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.Tests.Features.TaskItems.Commands.CreateTaskItem;

/// <summary>
/// Unit tests for <see cref="CreateTaskItemCommandHandler"/>.
/// </summary>
public class CreateTaskItemCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IRepository<Project>> _projectRepoMock;
    private readonly Mock<IRepository<TaskItem>> _taskItemRepoMock;
    private readonly CreateTaskItemCommandHandler _handler;

    public CreateTaskItemCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _projectRepoMock = new Mock<IRepository<Project>>();
        _taskItemRepoMock = new Mock<IRepository<TaskItem>>();

        _unitOfWorkMock.Setup(u => u.Projects).Returns(_projectRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.TaskItems).Returns(_taskItemRepoMock.Object);

        _handler = new CreateTaskItemCommandHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_Should_CreateTaskItem_WhenProjectExists()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var project = new Project { Id = projectId, Name = "Test Project" };

        _projectRepoMock
            .Setup(r => r.GetByIdAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        _taskItemRepoMock
            .Setup(r => r.AddAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskItem t, CancellationToken _) => t);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = new CreateTaskItemCommand("New Task", "Task Desc", projectId, TaskPriority.High, null);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _taskItemRepoMock.Verify(
            r => r.AddAsync(
                It.Is<TaskItem>(t =>
                    t.Title == "New Task" &&
                    t.ProjectId == projectId &&
                    t.Priority == TaskPriority.High),
                It.IsAny<CancellationToken>()),
            Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_ThrowNotFoundException_WhenProjectNotFound()
    {
        // Arrange
        _projectRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Project?)null);

        var command = new CreateTaskItemCommand("Task", "Desc", Guid.NewGuid(), TaskPriority.Medium, null);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
