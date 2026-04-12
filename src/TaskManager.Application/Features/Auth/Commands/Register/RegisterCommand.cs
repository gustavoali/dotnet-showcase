using MediatR;
using TaskManager.Application.Common.Models;

namespace TaskManager.Application.Features.Auth.Commands.Register;

/// <summary>
/// Command to register a new user account.
/// </summary>
public record RegisterCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName) : IRequest<AuthResult>;
