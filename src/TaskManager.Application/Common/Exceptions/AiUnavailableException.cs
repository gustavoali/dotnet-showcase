namespace TaskManager.Application.Common.Exceptions;

/// <summary>
/// Exception thrown when the AI assistant feature is unavailable — for example, when no API key is
/// configured or the upstream provider rejects authentication.
/// </summary>
public class AiUnavailableException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AiUnavailableException"/> class.
    /// </summary>
    public AiUnavailableException()
        : base("The AI assistant is not available.")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AiUnavailableException"/> class with a message.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public AiUnavailableException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AiUnavailableException"/> class with a message and inner exception.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">The inner exception.</param>
    public AiUnavailableException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
