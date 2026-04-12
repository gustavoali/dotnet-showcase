namespace TaskManager.Application.Common.Models;

/// <summary>
/// Represents the result of an authentication operation.
/// </summary>
public record AuthResult
{
    /// <summary>
    /// Gets a value indicating whether the authentication was successful.
    /// </summary>
    public bool Succeeded { get; init; }

    /// <summary>
    /// Gets the JWT token issued on successful authentication.
    /// </summary>
    public string Token { get; init; } = string.Empty;

    /// <summary>
    /// Gets the authenticated user's unique identifier.
    /// </summary>
    public string UserId { get; init; } = string.Empty;

    /// <summary>
    /// Gets the collection of error messages if authentication failed.
    /// </summary>
    public IEnumerable<string> Errors { get; init; } = Enumerable.Empty<string>();

    /// <summary>
    /// Creates a successful authentication result.
    /// </summary>
    /// <param name="token">The JWT token.</param>
    /// <param name="userId">The user's identifier.</param>
    /// <returns>A successful <see cref="AuthResult"/>.</returns>
    public static AuthResult Success(string token, string userId) =>
        new() { Succeeded = true, Token = token, UserId = userId };

    /// <summary>
    /// Creates a failed authentication result.
    /// </summary>
    /// <param name="errors">The error messages.</param>
    /// <returns>A failed <see cref="AuthResult"/>.</returns>
    public static AuthResult Failure(params string[] errors) =>
        new() { Succeeded = false, Errors = errors };
}
