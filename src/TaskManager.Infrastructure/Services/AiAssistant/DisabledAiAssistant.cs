using TaskManager.Application.Common.Exceptions;
using TaskManager.Application.Common.Interfaces;
using TaskManager.Application.DTOs;

namespace TaskManager.Infrastructure.Services.AiAssistant;

/// <summary>
/// A no-op <see cref="IAiAssistant"/> registered when no API key is configured. Every operation
/// throws <see cref="AiUnavailableException"/> so that AI endpoints degrade gracefully (HTTP 503)
/// while the rest of the API continues to function.
/// </summary>
public class DisabledAiAssistant : IAiAssistant
{
    private const string UnavailableMessage =
        "The AI assistant is not configured. Set the ANTHROPIC_API_KEY environment variable to enable it.";

    /// <inheritdoc/>
    public bool IsAvailable => false;

    /// <inheritdoc/>
    public Task<TaskDraftDto> DraftTaskAsync(string input, CancellationToken ct = default)
        => throw new AiUnavailableException(UnavailableMessage);

    /// <inheritdoc/>
    public IAsyncEnumerable<string> StreamSummaryAsync(ProjectSummaryContext context, CancellationToken ct = default)
        => throw new AiUnavailableException(UnavailableMessage);
}
