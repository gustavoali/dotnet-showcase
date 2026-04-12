using MediatR;
using TaskManager.Application.Common.Interfaces;
using TaskManager.Application.Common.Models;

namespace TaskManager.Application.Features.Auth.Commands.Register;

/// <summary>
/// Handles user registration.
/// </summary>
public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResult>
{
    private readonly IAuthService _authService;

    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterCommandHandler"/> class.
    /// </summary>
    /// <param name="authService">The authentication service.</param>
    public RegisterCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    /// <inheritdoc/>
    public async Task<AuthResult> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        return await _authService.RegisterAsync(
            request.Email,
            request.Password,
            request.FirstName,
            request.LastName,
            cancellationToken);
    }
}
