using Microsoft.Extensions.AI;

namespace TaskManager.Infrastructure.Tests.Services.AiAssistant;

/// <summary>
/// A test double for <see cref="IChatClient"/> that returns canned responses and streaming updates,
/// or throws a configured exception. Avoids the need to hit the real Anthropic API.
/// </summary>
internal sealed class FakeChatClient : IChatClient
{
    private readonly string? _responseText;
    private readonly IReadOnlyList<string>? _streamingChunks;
    private readonly Exception? _throwOnGetResponse;
    private readonly Exception? _throwOnGetStreaming;

    /// <summary>
    /// Captures the messages passed to the most recent call, for assertion.
    /// </summary>
    public List<ChatMessage> LastMessages { get; } = new();

    private FakeChatClient(
        string? responseText,
        IReadOnlyList<string>? streamingChunks,
        Exception? throwOnGetResponse,
        Exception? throwOnGetStreaming)
    {
        _responseText = responseText;
        _streamingChunks = streamingChunks;
        _throwOnGetResponse = throwOnGetResponse;
        _throwOnGetStreaming = throwOnGetStreaming;
    }

    /// <summary>
    /// Creates a fake client that returns the given text from <see cref="GetResponseAsync"/>.
    /// </summary>
    public static FakeChatClient WithResponse(string responseText)
        => new(responseText, null, null, null);

    /// <summary>
    /// Creates a fake client that yields the given chunks from <see cref="GetStreamingResponseAsync"/>.
    /// </summary>
    public static FakeChatClient WithStreaming(params string[] chunks)
        => new(null, chunks, null, null);

    /// <summary>
    /// Creates a fake client that throws the given exception from <see cref="GetResponseAsync"/>.
    /// </summary>
    public static FakeChatClient ThrowingOnResponse(Exception exception)
        => new(null, null, exception, null);

    /// <summary>
    /// Creates a fake client that throws the given exception while streaming.
    /// </summary>
    public static FakeChatClient ThrowingOnStreaming(Exception exception)
        => new(null, Array.Empty<string>(), null, exception);

    public Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        LastMessages.Clear();
        LastMessages.AddRange(messages);

        if (_throwOnGetResponse is not null)
        {
            throw _throwOnGetResponse;
        }

        var message = new ChatMessage(ChatRole.Assistant, _responseText ?? string.Empty);
        return Task.FromResult(new ChatResponse(message));
    }

    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        LastMessages.Clear();
        LastMessages.AddRange(messages);

        if (_throwOnGetStreaming is not null)
        {
            throw _throwOnGetStreaming;
        }

        foreach (var chunk in _streamingChunks ?? Array.Empty<string>())
        {
            yield return new ChatResponseUpdate(ChatRole.Assistant, chunk);
        }

        await Task.CompletedTask;
    }

    public object? GetService(Type serviceType, object? serviceKey = null) => null;

    public void Dispose()
    {
    }
}
