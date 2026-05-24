using FluentAssertions;
using Moq;
using TaskManager.Application.Common.Exceptions;
using TaskManager.Application.Common.Interfaces;
using TaskManager.Application.DTOs;
using TaskManager.Application.Features.AiAssistant.Commands.DraftTask;
using TaskManager.Domain.Enums;

namespace TaskManager.Application.Tests.Features.AiAssistant.Commands.DraftTask;

/// <summary>
/// Unit tests for <see cref="DraftTaskCommandHandler"/>.
/// </summary>
public class DraftTaskCommandHandlerTests
{
    private readonly Mock<IAiAssistant> _aiAssistantMock;
    private readonly DraftTaskCommandHandler _handler;

    public DraftTaskCommandHandlerTests()
    {
        _aiAssistantMock = new Mock<IAiAssistant>();
        _handler = new DraftTaskCommandHandler(_aiAssistantMock.Object);
    }

    [Fact]
    public async Task Handle_Should_DelegateToAiAssistant_And_ReturnDraft()
    {
        // Arrange
        var command = new DraftTaskCommand("pay suppliers on Friday, urgent");
        var expected = new TaskDraftDto("Pay suppliers", "Pay suppliers by Friday", TaskPriority.High, null);

        _aiAssistantMock
            .Setup(a => a.DraftTaskAsync(command.Input, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(expected);
        _aiAssistantMock.Verify(a => a.DraftTaskAsync(command.Input, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_PassInput_Verbatim()
    {
        // Arrange
        var command = new DraftTaskCommand("write the quarterly report");
        string? capturedInput = null;

        _aiAssistantMock
            .Setup(a => a.DraftTaskAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<string, CancellationToken>((input, _) => capturedInput = input)
            .ReturnsAsync(new TaskDraftDto("Report", string.Empty, TaskPriority.Medium, null));

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedInput.Should().Be("write the quarterly report");
    }

    [Fact]
    public async Task Handle_Should_Propagate_AiUnavailableException()
    {
        // Arrange
        var command = new DraftTaskCommand("anything");
        _aiAssistantMock
            .Setup(a => a.DraftTaskAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AiUnavailableException());

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<AiUnavailableException>();
    }
}
