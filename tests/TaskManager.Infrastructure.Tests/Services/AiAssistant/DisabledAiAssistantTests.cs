using FluentAssertions;
using TaskManager.Application.Common.Exceptions;
using TaskManager.Application.DTOs;
using TaskManager.Infrastructure.Services.AiAssistant;

namespace TaskManager.Infrastructure.Tests.Services.AiAssistant;

/// <summary>
/// Unit tests for <see cref="DisabledAiAssistant"/>.
/// </summary>
public class DisabledAiAssistantTests
{
    private readonly DisabledAiAssistant _sut = new();

    [Fact]
    public void IsAvailable_Should_BeFalse()
    {
        _sut.IsAvailable.Should().BeFalse();
    }

    [Fact]
    public void StreamSummaryAsync_Should_Throw_Eagerly_BeforeEnumeration()
    {
        // Arrange
        var context = new ProjectSummaryContext("P", "d", new List<ProjectSummaryTask>());

        // Act — the throw must occur on the call itself, NOT deferred to enumeration. This is the
        // contract that lets the API return 503 before any response headers are flushed.
        var act = () => _sut.StreamSummaryAsync(context);

        // Assert
        act.Should().Throw<AiUnavailableException>();
    }

    [Fact]
    public async Task DraftTaskAsync_Should_Throw_AiUnavailableException()
    {
        // Act
        var act = () => _sut.DraftTaskAsync("anything");

        // Assert
        await act.Should().ThrowAsync<AiUnavailableException>();
    }

    [Fact]
    public async Task StreamSummaryAsync_Should_Throw_AiUnavailableException()
    {
        // Arrange
        var context = new ProjectSummaryContext("P", "d", new List<ProjectSummaryTask>());

        // Act
        var act = async () =>
        {
            await foreach (var _ in _sut.StreamSummaryAsync(context))
            {
                // drain
            }
        };

        // Assert
        await act.Should().ThrowAsync<AiUnavailableException>();
    }
}
