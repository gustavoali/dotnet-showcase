namespace TaskManager.Application.Common.Exceptions;

/// <summary>
/// Exception thrown when a user attempts to access a resource they are not authorized to access.
/// </summary>
public class ForbiddenAccessException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ForbiddenAccessException"/> class.
    /// </summary>
    public ForbiddenAccessException()
        : base("Access to this resource is forbidden.")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ForbiddenAccessException"/> class with a message.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public ForbiddenAccessException(string message)
        : base(message)
    {
    }
}
