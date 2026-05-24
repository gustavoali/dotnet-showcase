using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using TaskManager.Application.Common.Exceptions;
using TaskManager.Application.DTOs;
using TaskManager.Domain.Enums;
using TaskManager.Infrastructure.Services.AiAssistant;
using AnthropicExceptions = Anthropic.Exceptions;

namespace TaskManager.Infrastructure.Tests.Services.AiAssistant;

/// <summary>
/// Unit tests for <see cref="AnthropicAiAssistant"/> using a fake <see cref="FakeChatClient"/>.
/// </summary>
public class AnthropicAiAssistantTests
{
    private static AnthropicAiAssistant CreateSut(FakeChatClient chatClient)
    {
        var options = Options.Create(new AiOptions { Model = "claude-haiku-4-5", MaxTokens = 1024 });
        return new AnthropicAiAssistant(chatClient, options, NullLogger<AnthropicAiAssistant>.Instance);
    }

    [Fact]
    public void IsAvailable_Should_BeTrue()
    {
        // Arrange
        var sut = CreateSut(FakeChatClient.WithResponse("{}"));

        // Assert
        sut.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public async Task DraftTaskAsync_Should_MapPlainJson_ToDto()
    {
        // Arrange
        const string json = """
            {"title":"Pay suppliers","description":"Pay all outstanding invoices","priority":"High","dueDate":"2026-06-05T00:00:00Z"}
            """;
        var sut = CreateSut(FakeChatClient.WithResponse(json));

        // Act
        var result = await sut.DraftTaskAsync("pay suppliers on Friday, urgent");

        // Assert
        result.Title.Should().Be("Pay suppliers");
        result.Description.Should().Be("Pay all outstanding invoices");
        result.Priority.Should().Be(TaskPriority.High);
        result.DueDate.Should().Be(new DateTime(2026, 6, 5, 0, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public async Task DraftTaskAsync_Should_StripMarkdownFences_BeforeParsing()
    {
        // Arrange
        const string fenced = "```json\n{\"title\":\"Write report\",\"description\":\"\",\"priority\":\"Low\",\"dueDate\":null}\n```";
        var sut = CreateSut(FakeChatClient.WithResponse(fenced));

        // Act
        var result = await sut.DraftTaskAsync("write a report");

        // Assert
        result.Title.Should().Be("Write report");
        result.Description.Should().BeEmpty();
        result.Priority.Should().Be(TaskPriority.Low);
        result.DueDate.Should().BeNull();
    }

    [Theory]
    [InlineData("Critical", TaskPriority.Critical)]
    [InlineData("critical", TaskPriority.Critical)]
    [InlineData("HIGH", TaskPriority.High)]
    [InlineData("unknown-value", TaskPriority.Medium)]
    [InlineData("", TaskPriority.Medium)]
    public async Task DraftTaskAsync_Should_ParsePriority_Leniently(string priorityText, TaskPriority expected)
    {
        // Arrange
        var json = $"{{\"title\":\"T\",\"description\":\"\",\"priority\":\"{priorityText}\",\"dueDate\":null}}";
        var sut = CreateSut(FakeChatClient.WithResponse(json));

        // Act
        var result = await sut.DraftTaskAsync("anything");

        // Assert
        result.Priority.Should().Be(expected);
    }

    [Fact]
    public async Task DraftTaskAsync_Should_TolerateMissingDueDate()
    {
        // Arrange
        const string json = """{"title":"T","description":"d","priority":"Medium"}""";
        var sut = CreateSut(FakeChatClient.WithResponse(json));

        // Act
        var result = await sut.DraftTaskAsync("anything");

        // Assert
        result.DueDate.Should().BeNull();
    }

    [Fact]
    public async Task DraftTaskAsync_Should_ThrowAiResponseException_WhenResponseIsNotJson()
    {
        // Arrange — the service IS available; only the payload is bad, so this is 502 (not 503).
        var sut = CreateSut(FakeChatClient.WithResponse("this is not json at all"));

        // Act
        var act = () => sut.DraftTaskAsync("anything");

        // Assert
        await act.Should().ThrowAsync<AiResponseException>();
    }

    [Fact]
    public async Task DraftTaskAsync_Should_ThrowAiResponseException_WhenTitleMissing()
    {
        // Arrange
        const string json = """{"description":"d","priority":"Low","dueDate":null}""";
        var sut = CreateSut(FakeChatClient.WithResponse(json));

        // Act
        var act = () => sut.DraftTaskAsync("anything");

        // Assert
        await act.Should().ThrowAsync<AiResponseException>();
    }

    [Fact]
    public async Task DraftTaskAsync_Should_ParseSingleLineFencedJson()
    {
        // Arrange — fenced JSON with no surrounding newlines (M-2).
        const string fenced = "```json {\"title\":\"Single line\",\"description\":\"\",\"priority\":\"Medium\",\"dueDate\":null} ```";
        var sut = CreateSut(FakeChatClient.WithResponse(fenced));

        // Act
        var result = await sut.DraftTaskAsync("anything");

        // Assert
        result.Title.Should().Be("Single line");
        result.Priority.Should().Be(TaskPriority.Medium);
    }

    [Fact]
    public async Task DraftTaskAsync_Should_ParseJsonEmbeddedInProse()
    {
        // Arrange — JSON wrapped in conversational prose (M-2).
        const string prose = "Sure! Here is the task you asked for: {\"title\":\"From prose\",\"description\":\"d\",\"priority\":\"High\",\"dueDate\":null} Let me know if you need anything else.";
        var sut = CreateSut(FakeChatClient.WithResponse(prose));

        // Act
        var result = await sut.DraftTaskAsync("anything");

        // Assert
        result.Title.Should().Be("From prose");
        result.Priority.Should().Be(TaskPriority.High);
    }

    [Fact]
    public async Task DraftTaskAsync_Should_NotDefaultToToday_ForTimeOnlyDueDate()
    {
        // Arrange — a time-only due date must NOT silently acquire today's date (L-2). With
        // NoCurrentDateDefault the parser uses year 1, which we then treat as a non-meaningful date.
        const string json = """{"title":"T","description":"","priority":"Medium","dueDate":"14:30"}""";
        var sut = CreateSut(FakeChatClient.WithResponse(json));

        // Act
        var result = await sut.DraftTaskAsync("anything");

        // Assert — the parsed date must not be today's date.
        if (result.DueDate.HasValue)
        {
            result.DueDate.Value.Date.Should().NotBe(DateTime.UtcNow.Date);
        }
    }

    [Fact]
    public async Task DraftTaskAsync_Should_MapUnauthorized_ToAiUnavailable()
    {
        // Arrange
        var sut = CreateSut(FakeChatClient.ThrowingOnResponse(BuildUnauthorizedException()));

        // Act
        var act = () => sut.DraftTaskAsync("anything");

        // Assert
        await act.Should().ThrowAsync<AiUnavailableException>();
    }

    [Fact]
    public async Task StreamSummaryAsync_Should_YieldChunks()
    {
        // Arrange
        var sut = CreateSut(FakeChatClient.WithStreaming("Hello ", "world", string.Empty, "!"));
        var context = new ProjectSummaryContext(
            "Launch",
            "Launch the product",
            new List<ProjectSummaryTask>
            {
                new("Build", TaskItemStatus.InProgress, TaskPriority.High, null)
            });

        // Act
        var chunks = new List<string>();
        await foreach (var chunk in sut.StreamSummaryAsync(context))
        {
            chunks.Add(chunk);
        }

        // Assert — empty chunks are filtered out
        chunks.Should().Equal("Hello ", "world", "!");
    }

    [Fact]
    public async Task StreamSummaryAsync_Should_MapUnauthorized_ToAiUnavailable()
    {
        // Arrange
        var sut = CreateSut(FakeChatClient.ThrowingOnStreaming(BuildUnauthorizedException()));
        var context = new ProjectSummaryContext("P", "d", new List<ProjectSummaryTask>());

        // Act
        var act = async () =>
        {
            await foreach (var _ in sut.StreamSummaryAsync(context))
            {
                // drain
            }
        };

        // Assert
        await act.Should().ThrowAsync<AiUnavailableException>();
    }

    private static AnthropicExceptions.AnthropicUnauthorizedException BuildUnauthorizedException()
    {
        // The SDK's only public constructor is obsolete and leaves required members unset, so set
        // them via an object initializer to construct a valid instance for the mapping test.
#pragma warning disable CS0618 // Type or member is obsolete
        return new AnthropicExceptions.AnthropicUnauthorizedException(
            new HttpRequestException("401 Unauthorized"))
        {
            StatusCode = System.Net.HttpStatusCode.Unauthorized,
            ResponseBody = "{\"error\":\"unauthorized\"}"
        };
#pragma warning restore CS0618
    }
}
