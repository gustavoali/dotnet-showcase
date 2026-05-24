using FluentAssertions;
using Moq;
using TaskManager.Application.Common.Exceptions;
using TaskManager.Application.Common.Interfaces;
using TaskManager.Application.DTOs;
using TaskManager.Application.Features.AiAssistant.Queries.StreamProjectSummary;
using TaskManager.Application.Tests.Helpers;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.Tests.Features.AiAssistant.Queries.StreamProjectSummary;

/// <summary>
/// Unit tests for <see cref="StreamProjectSummaryQueryHandler"/>.
/// </summary>
public class StreamProjectSummaryQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IRepository<Project>> _projectRepoMock;
    private readonly Mock<IAiAssistant> _aiAssistantMock;
    private readonly StreamProjectSummaryQueryHandler _handler;

    public StreamProjectSummaryQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _projectRepoMock = new Mock<IRepository<Project>>();
        _aiAssistantMock = new Mock<IAiAssistant>();

        _unitOfWorkMock.Setup(u => u.Projects).Returns(_projectRepoMock.Object);

        // By default the assistant is available; specific tests override this.
        _aiAssistantMock.SetupGet(a => a.IsAvailable).Returns(true);

        _handler = new StreamProjectSummaryQueryHandler(_unitOfWorkMock.Object, _aiAssistantMock.Object);
    }

    [Fact]
    public async Task Handle_Should_ThrowAiUnavailableException_WhenAssistantNotAvailable()
    {
        // Arrange — the eager availability check must fire BEFORE touching the database, so the
        // controller can return 503 before any streaming headers are flushed.
        _aiAssistantMock.SetupGet(a => a.IsAvailable).Returns(false);
        var query = new StreamProjectSummaryQuery(Guid.NewGuid());

        // Act
        var act = () => _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<AiUnavailableException>();
        _projectRepoMock.Verify(r => r.Query(), Times.Never);
        _aiAssistantMock.Verify(
            a => a.StreamSummaryAsync(It.IsAny<ProjectSummaryContext>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_Should_ThrowNotFoundException_WhenProjectNotFound()
    {
        // Arrange
        _projectRepoMock.Setup(r => r.Query()).Returns(new List<Project>().AsAsyncQueryable());
        var query = new StreamProjectSummaryQuery(Guid.NewGuid());

        // Act
        var act = () => _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _aiAssistantMock.Verify(
            a => a.StreamSummaryAsync(It.IsAny<ProjectSummaryContext>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_Should_BuildContext_FromProjectAndTasks_AndDelegateToAssistant()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var dueDate = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc);
        var project = new Project
        {
            Id = projectId,
            Name = "Launch",
            Description = "Launch the product",
            Tasks = new List<TaskItem>
            {
                new() { Id = Guid.NewGuid(), Title = "Build", Status = TaskItemStatus.InProgress, Priority = TaskPriority.High, DueDate = dueDate },
                new() { Id = Guid.NewGuid(), Title = "Test", Status = TaskItemStatus.Todo, Priority = TaskPriority.Critical, DueDate = null }
            }
        };

        _projectRepoMock.Setup(r => r.Query()).Returns(new List<Project> { project }.AsAsyncQueryable());

        ProjectSummaryContext? capturedContext = null;
        _aiAssistantMock
            .Setup(a => a.StreamSummaryAsync(It.IsAny<ProjectSummaryContext>(), It.IsAny<CancellationToken>()))
            .Callback<ProjectSummaryContext, CancellationToken>((ctx, _) => capturedContext = ctx)
            .Returns(ToAsyncEnumerable("summary"));

        var query = new StreamProjectSummaryQuery(projectId);

        // Act
        var stream = await _handler.Handle(query, CancellationToken.None);
        var chunks = await CollectAsync(stream);

        // Assert
        capturedContext.Should().NotBeNull();
        capturedContext!.ProjectName.Should().Be("Launch");
        capturedContext.ProjectDescription.Should().Be("Launch the product");
        capturedContext.Tasks.Should().HaveCount(2);
        capturedContext.Tasks.Should().ContainSingle(t =>
            t.Title == "Build" && t.Status == TaskItemStatus.InProgress && t.Priority == TaskPriority.High && t.DueDate == dueDate);
        capturedContext.Tasks.Should().ContainSingle(t =>
            t.Title == "Test" && t.Status == TaskItemStatus.Todo && t.Priority == TaskPriority.Critical && t.DueDate == null);
        chunks.Should().ContainSingle().Which.Should().Be("summary");
    }

    private static async IAsyncEnumerable<string> ToAsyncEnumerable(params string[] items)
    {
        foreach (var item in items)
        {
            yield return item;
        }

        await Task.CompletedTask;
    }

    private static async Task<List<string>> CollectAsync(IAsyncEnumerable<string> source)
    {
        var result = new List<string>();
        await foreach (var item in source)
        {
            result.Add(item);
        }

        return result;
    }
}
