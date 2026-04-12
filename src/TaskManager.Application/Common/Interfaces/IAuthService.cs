using TaskManager.Application.Common.Models;

namespace TaskManager.Application.Common.Interfaces;

/// <summary>
/// Service for authentication operations including registration and login.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Registers a new user account.
    /// </summary>
    /// <param name="email">The user's email address.</param>
    /// <param name="password">The user's password.</param>
    /// <param name="firstName">The user's first name.</param>
    /// <param name="lastName">The user's last name.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The authentication result containing a JWT token on success.</returns>
    Task<AuthResult> RegisterAsync(string email, string password, string firstName, string lastName, CancellationToken ct = default);

    /// <summary>
    /// Authenticates a user with email and password.
    /// </summary>
    /// <param name="email">The user's email address.</param>
    /// <param name="password">The user's password.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The authentication result containing a JWT token on success.</returns>
    Task<AuthResult> LoginAsync(string email, string password, CancellationToken ct = default);
}
