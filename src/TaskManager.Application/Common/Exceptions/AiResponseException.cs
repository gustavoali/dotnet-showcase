namespace TaskManager.Application.Common.Exceptions;

/// <summary>
/// Exception thrown when the AI assistant is reachable and responded, but its output could not be
/// interpreted — for example, malformed JSON or a draft missing required fields. This represents a
/// faulty upstream payload (mapped to HTTP 502 Bad Gateway), distinct from the assistant being
/// unavailable (<see cref="AiUnavailableException"/>, HTTP 503).
/// </summary>
public class AiResponseException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AiResponseException"/> class.
    /// </summary>
    public AiResponseException()
        : base("The AI assistant returned a response that could not be processed.")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AiResponseException"/> class with a message.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public AiResponseException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AiResponseException"/> class with a message and inner exception.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">The inner exception.</param>
    public AiResponseException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
