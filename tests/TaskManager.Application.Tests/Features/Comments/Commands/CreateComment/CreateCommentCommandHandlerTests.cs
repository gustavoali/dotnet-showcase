using FluentAssertions;
using Moq;
using TaskManager.Application.Common.Exceptions;
using TaskManager.Application.Common.Interfaces;
using TaskManager.Application.Features.Comments.Commands.CreateComment;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.Tests.Features.Comments.Commands.CreateComment;

public class CreateCommentCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IRepository<TaskItem>> _taskItemRepoMock;
    private readonly Mock<IRepository<Comment>> _commentRepoMock;
    private readonly Mock<IRepository<User>> _userRepoMock;
    private readonly CreateCommentCommandHandler _handler;
    private readonly Guid _userId = Guid.NewGuid();

    public CreateCommentCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _taskItemRepoMock = new Mock<IRepository<TaskItem>>();
        _commentRepoMock = new Mock<IRepository<Comment>>();
        _userRepoMock = new Mock<IRepository<User>>();

        _unitOfWorkMock.Setup(u => u.TaskItems).Returns(_taskItemRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Comments).Returns(_commentRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Users).Returns(_userRepoMock.Object);
        _currentUserServiceMock.Setup(c => c.UserId).Returns(_userId);

        _handler = new CreateCommentCommandHandler(
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_Should_CreateComment_WhenTaskItemExists()
    {
        // Arrange
        var taskItemId = Guid.NewGuid();
        var taskItem = new TaskItem { Id = taskItemId, Title = "Test Task" };
        var user = new User { Id = _userId, FirstName = "John", LastName = "Doe" };

        _taskItemRepoMock
            .Setup(r => r.GetByIdAsync(taskItemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(taskItem);

        _commentRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Comment>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Comment c, CancellationToken _) => c);

        _userRepoMock
            .Setup(r => r.GetByIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = new CreateCommentCommand(taskItemId, "This is a comment.");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.TaskItemId.Should().Be(taskItemId);
        result.AuthorId.Should().Be(_userId);
        result.Content.Should().Be("This is a comment.");
        result.AuthorName.Should().Be("John Doe");
        _commentRepoMock.Verify(r => r.AddAsync(It.IsAny<Comment>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_SetAuthorId_FromCurrentUserService()
    {
        // Arrange
        var taskItemId = Guid.NewGuid();
        var taskItem = new TaskItem { Id = taskItemId, Title = "Test Task" };
        Comment? capturedComment = null;

        _taskItemRepoMock
            .Setup(r => r.GetByIdAsync(taskItemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(taskItem);

        _commentRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Comment>(), It.IsAny<CancellationToken>()))
            .Callback<Comment, CancellationToken>((c, _) => capturedComment = c)
            .ReturnsAsync((Comment c, CancellationToken _) => c);

        _userRepoMock
            .Setup(r => r.GetByIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = _userId, FirstName = "John", LastName = "Doe" });

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = new CreateCommentCommand(taskItemId, "Comment content");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedComment.Should().NotBeNull();
        capturedComment!.AuthorId.Should().Be(_userId);
    }

    [Fact]
    public async Task Handle_Should_ThrowNotFoundException_WhenTaskItemNotFound()
    {
        // Arrange
        _taskItemRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskItem?)null);

        var command = new CreateCommentCommand(Guid.NewGuid(), "Comment content");

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_Should_SetCreatedAt_ToUtcNow()
    {
        // Arrange
        var taskItemId = Guid.NewGuid();
        var taskItem = new TaskItem { Id = taskItemId, Title = "Test Task" };
        Comment? capturedComment = null;
        var beforeUtc = DateTime.UtcNow;

        _taskItemRepoMock
            .Setup(r => r.GetByIdAsync(taskItemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(taskItem);

        _commentRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Comment>(), It.IsAny<CancellationToken>()))
            .Callback<Comment, CancellationToken>((c, _) => capturedComment = c)
            .ReturnsAsync((Comment c, CancellationToken _) => c);

        _userRepoMock
            .Setup(r => r.GetByIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = _userId, FirstName = "John", LastName = "Doe" });

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = new CreateCommentCommand(taskItemId, "Comment content");

        // Act
        await _handler.Handle(command, CancellationToken.None);
        var afterUtc = DateTime.UtcNow;

        // Assert
        capturedComment.Should().NotBeNull();
        capturedComment!.CreatedAt.Should().BeOnOrAfter(beforeUtc).And.BeOnOrBefore(afterUtc);
    }
}
