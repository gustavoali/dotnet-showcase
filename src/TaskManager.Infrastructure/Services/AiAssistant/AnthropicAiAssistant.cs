using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaskManager.Application.Common.Exceptions;
using TaskManager.Application.Common.Interfaces;
using TaskManager.Application.DTOs;
using TaskManager.Domain.Enums;
using AnthropicExceptions = Anthropic.Exceptions;

namespace TaskManager.Infrastructure.Services.AiAssistant;

/// <summary>
/// AI assistant implementation backed by the official Anthropic SDK via the
/// <see cref="IChatClient"/> abstraction. This is the only type that depends on the Anthropic SDK;
/// the rest of the application talks to <see cref="IAiAssistant"/>.
/// </summary>
public class AnthropicAiAssistant : IAiAssistant
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IChatClient _chatClient;
    private readonly AiOptions _options;
    private readonly ILogger<AnthropicAiAssistant> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AnthropicAiAssistant"/> class.
    /// </summary>
    /// <param name="chatClient">The chat client backed by the Anthropic SDK.</param>
    /// <param name="options">The AI configuration options.</param>
    /// <param name="logger">The logger.</param>
    public AnthropicAiAssistant(
        IChatClient chatClient,
        IOptions<AiOptions> options,
        ILogger<AnthropicAiAssistant> logger)
    {
        _chatClient = chatClient;
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc/>
    public bool IsAvailable => true;

    /// <inheritdoc/>
    public async Task<TaskDraftDto> DraftTaskAsync(string input, CancellationToken ct = default)
    {
        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, BuildDraftSystemPrompt()),
            new(ChatRole.User, input)
        };

        var chatOptions = new ChatOptions { MaxOutputTokens = _options.MaxTokens };

        var stopwatch = Stopwatch.StartNew();
        ChatResponse response;
        try
        {
            response = await _chatClient.GetResponseAsync(messages, chatOptions, ct);
        }
        catch (AnthropicExceptions.AnthropicUnauthorizedException ex)
        {
            throw new AiUnavailableException("The AI assistant rejected the configured credentials.", ex);
        }
        finally
        {
            stopwatch.Stop();
        }

        // Log only metadata (model, latency, token counts) — never prompts, inputs, or outputs.
        _logger.LogInformation(
            "AI task draft completed. Model={Model}, ElapsedMs={ElapsedMs}, InputTokens={InputTokens}, OutputTokens={OutputTokens}",
            _options.Model,
            stopwatch.ElapsedMilliseconds,
            response.Usage?.InputTokenCount,
            response.Usage?.OutputTokenCount);

        return ParseDraft(response.Text);
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<string> StreamSummaryAsync(
        ProjectSummaryContext context,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, BuildSummarySystemPrompt()),
            new(ChatRole.User, BuildSummaryUserPrompt(context))
        };

        var chatOptions = new ChatOptions { MaxOutputTokens = _options.MaxTokens };

        var stopwatch = Stopwatch.StartNew();
        long? inputTokens = null;
        long? outputTokens = null;

        IAsyncEnumerator<ChatResponseUpdate> enumerator;
        try
        {
            enumerator = _chatClient.GetStreamingResponseAsync(messages, chatOptions, ct).GetAsyncEnumerator(ct);
        }
        catch (AnthropicExceptions.AnthropicUnauthorizedException ex)
        {
            throw new AiUnavailableException("The AI assistant rejected the configured credentials.", ex);
        }

        try
        {
            while (true)
            {
                ChatResponseUpdate update;
                try
                {
                    if (!await enumerator.MoveNextAsync())
                    {
                        break;
                    }

                    update = enumerator.Current;
                }
                catch (AnthropicExceptions.AnthropicUnauthorizedException ex)
                {
                    throw new AiUnavailableException("The AI assistant rejected the configured credentials.", ex);
                }

                // Token usage typically arrives on the terminal update; capture it when present.
                var usage = update.Contents.OfType<UsageContent>().FirstOrDefault()?.Details;
                if (usage is not null)
                {
                    inputTokens = usage.InputTokenCount;
                    outputTokens = usage.OutputTokenCount;
                }

                if (!string.IsNullOrEmpty(update.Text))
                {
                    yield return update.Text;
                }
            }
        }
        finally
        {
            await enumerator.DisposeAsync();

            stopwatch.Stop();

            // Logged in finally so latency is recorded even on early cancellation or abandonment.
            // Log only metadata (model, latency, token counts) — never prompts, inputs, or outputs.
            _logger.LogInformation(
                "AI project summary stream completed. Model={Model}, ElapsedMs={ElapsedMs}, InputTokens={InputTokens}, OutputTokens={OutputTokens}",
                _options.Model,
                stopwatch.ElapsedMilliseconds,
                inputTokens,
                outputTokens);
        }
    }

    private static string BuildDraftSystemPrompt()
    {
        return """
            You are a task-drafting assistant. Convert the user's natural-language request into a single
            task. Respond with ONLY a JSON object (no prose, no markdown code fences) matching exactly:
            {
              "title": string,        // a short, action-oriented title
              "description": string,  // a concise description; may be empty
              "priority": string,     // one of: "Low", "Medium", "High", "Critical"
              "dueDate": string|null  // ISO-8601 date-time if a due date is implied, otherwise null
            }
            Infer priority from urgency cues in the input. If no due date is implied, use null.
            """;
    }

    private static string BuildSummarySystemPrompt()
    {
        return """
            You are a project-management assistant. Given a project and its tasks, write a brief,
            natural-language summary (2-4 sentences) of the project's current status: progress,
            notable blockers or overdue items, and the highest-priority outstanding work.
            Write plain prose. Do not use markdown headings or bullet lists.
            """;
    }

    private static string BuildSummaryUserPrompt(ProjectSummaryContext context)
    {
        var lines = new List<string>
        {
            $"Project: {context.ProjectName}",
            $"Description: {context.ProjectDescription}",
            $"Task count: {context.Tasks.Count}",
            "Tasks:"
        };

        if (context.Tasks.Count == 0)
        {
            lines.Add("(none)");
        }
        else
        {
            foreach (var task in context.Tasks)
            {
                var due = task.DueDate.HasValue
                    ? task.DueDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                    : "no due date";
                lines.Add($"- {task.Title} [status: {task.Status}, priority: {task.Priority}, due: {due}]");
            }
        }

        return string.Join('\n', lines);
    }

    /// <summary>
    /// Parses the model's JSON response into a <see cref="TaskDraftDto"/>, tolerating markdown
    /// code fences and lenient priority/date formats.
    /// </summary>
    /// <param name="rawText">The raw text returned by the model.</param>
    /// <returns>The parsed <see cref="TaskDraftDto"/>.</returns>
    /// <exception cref="AiResponseException">
    /// Thrown when the response cannot be parsed or is missing required fields. The assistant is
    /// reachable; only its payload is faulty, so this maps to HTTP 502 rather than 503.
    /// </exception>
    private static TaskDraftDto ParseDraft(string rawText)
    {
        var json = StripMarkdownFences(rawText);

        DraftPayload? payload;
        try
        {
            payload = JsonSerializer.Deserialize<DraftPayload>(json, JsonOptions);
        }
        catch (JsonException ex)
        {
            throw new AiResponseException("The AI assistant returned a response that could not be parsed.", ex);
        }

        if (payload is null || string.IsNullOrWhiteSpace(payload.Title))
        {
            throw new AiResponseException("The AI assistant returned an incomplete task draft.");
        }

        return new TaskDraftDto(
            payload.Title.Trim(),
            (payload.Description ?? string.Empty).Trim(),
            ParsePriority(payload.Priority),
            ParseDueDate(payload.DueDate));
    }

    /// <summary>
    /// Extracts the JSON payload from the model's raw text, tolerating markdown code fences (multi-line
    /// or single-line), language hints, and JSON embedded in surrounding prose. Falls back to the
    /// substring between the first <c>{</c> and the last <c>}</c> when fence stripping is insufficient.
    /// </summary>
    /// <param name="text">The raw text.</param>
    /// <returns>The extracted JSON candidate.</returns>
    private static string StripMarkdownFences(string text)
    {
        var trimmed = (text ?? string.Empty).Trim();

        if (trimmed.StartsWith("```", StringComparison.Ordinal))
        {
            var firstNewline = trimmed.IndexOf('\n');
            if (firstNewline >= 0)
            {
                // Multi-line fence: drop the opening fence line (which may include a hint like ```json)
                // and any trailing closing fence.
                var withoutOpening = trimmed[(firstNewline + 1)..];
                var closingFence = withoutOpening.LastIndexOf("```", StringComparison.Ordinal);
                if (closingFence >= 0)
                {
                    withoutOpening = withoutOpening[..closingFence];
                }

                trimmed = withoutOpening.Trim();
            }
        }

        // Fallback: if the candidate is still not a bare JSON object (e.g. single-line fenced JSON or
        // JSON wrapped in prose), extract the substring between the first '{' and the last '}'.
        if (!trimmed.StartsWith('{') || !trimmed.EndsWith('}'))
        {
            var start = trimmed.IndexOf('{');
            var end = trimmed.LastIndexOf('}');
            if (start >= 0 && end > start)
            {
                return trimmed[start..(end + 1)];
            }
        }

        return trimmed;
    }

    /// <summary>
    /// Maps a priority string from the model to a <see cref="TaskPriority"/>, defaulting to
    /// <see cref="TaskPriority.Medium"/> when the value is missing or unrecognized.
    /// </summary>
    /// <param name="priority">The priority string from the model.</param>
    /// <returns>The mapped <see cref="TaskPriority"/>.</returns>
    private static TaskPriority ParsePriority(string? priority)
    {
        if (!string.IsNullOrWhiteSpace(priority)
            && Enum.TryParse<TaskPriority>(priority.Trim(), ignoreCase: true, out var parsed))
        {
            return parsed;
        }

        return TaskPriority.Medium;
    }

    /// <summary>
    /// Parses an optional ISO-8601 due date, returning <see langword="null"/> when absent or invalid.
    /// </summary>
    /// <param name="dueDate">The due date string from the model.</param>
    /// <returns>The parsed UTC <see cref="DateTime"/>, or <see langword="null"/>.</returns>
    private static DateTime? ParseDueDate(string? dueDate)
    {
        if (string.IsNullOrWhiteSpace(dueDate)
            || string.Equals(dueDate.Trim(), "null", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        if (DateTime.TryParse(
                dueDate.Trim(),
                CultureInfo.InvariantCulture,
                DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal | DateTimeStyles.NoCurrentDateDefault,
                out var parsed))
        {
            return parsed;
        }

        return null;
    }

    /// <summary>
    /// Internal shape used to deserialize the model's JSON task draft.
    /// </summary>
    private sealed record DraftPayload
    {
        public string? Title { get; init; }

        public string? Description { get; init; }

        public string? Priority { get; init; }

        public string? DueDate { get; init; }
    }
}
