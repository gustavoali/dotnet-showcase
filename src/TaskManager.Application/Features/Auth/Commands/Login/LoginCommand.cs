using MediatR;
using TaskManager.Application.Common.Models;

namespace TaskManager.Application.Features.Auth.Commands.Login;

/// <summary>
/// Command to authenticate a user.
/// </summary>
public record LoginCommand(
    string Email,
    string Password) : IRequest<AuthResult>;
