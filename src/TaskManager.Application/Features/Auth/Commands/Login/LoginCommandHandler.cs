using MediatR;
using TaskManager.Application.Common.Interfaces;
using TaskManager.Application.Common.Models;

namespace TaskManager.Application.Features.Auth.Commands.Login;

/// <summary>
/// Handles user authentication.
/// </summary>
public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResult>
{
    private readonly IAuthService _authService;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoginCommandHandler"/> class.
    /// </summary>
    /// <param name="authService">The authentication service.</param>
    public LoginCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    /// <inheritdoc/>
    public async Task<AuthResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        return await _authService.LoginAsync(request.Email, request.Password, cancellationToken);
    }
}
