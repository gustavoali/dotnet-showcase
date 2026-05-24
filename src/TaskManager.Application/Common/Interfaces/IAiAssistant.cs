using TaskManager.Application.DTOs;

namespace TaskManager.Application.Common.Interfaces;

/// <summary>
/// Abstraction over the AI assistant used by the application layer. Implementations live in the
/// Infrastructure layer and encapsulate the underlying large-language-model client, keeping AI
/// provider types out of the Application layer.
/// </summary>
public interface IAiAssistant
{
    /// <summary>
    /// Gets a value indicating whether the assistant is configured and usable. Callers should check
    /// this before invoking <see cref="StreamSummaryAsync"/> so that an unavailable assistant fails
    /// eagerly (allowing the API to set HTTP 503 before any streaming response headers are flushed).
    /// </summary>
    bool IsAvailable { get; }

    /// <summary>
    /// Drafts a structured task suggestion from free-form natural-language input.
    /// </summary>
    /// <param name="input">The natural-language description of the task to draft.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A structured <see cref="TaskDraftDto"/> with the suggested fields.</returns>
    Task<TaskDraftDto> DraftTaskAsync(string input, CancellationToken ct = default);

    /// <summary>
    /// Streams a natural-language summary of a project's task status.
    /// </summary>
    /// <param name="context">The persistence-agnostic project context to summarize.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>An asynchronous stream of text chunks composing the summary.</returns>
    /// <remarks>
    /// The deterministic not-configured case is caught eagerly via <see cref="IsAvailable"/> and
    /// surfaces as HTTP 503 before any bytes are written. However, if the upstream provider rejects
    /// the request <em>after</em> the first chunk has been flushed to the client (for example, a
    /// credential revoked mid-stream), the HTTP status can no longer be changed — this is an inherent
    /// limitation of streaming responses, and the stream simply terminates.
    /// </remarks>
    IAsyncEnumerable<string> StreamSummaryAsync(ProjectSummaryContext context, CancellationToken ct = default);
}
