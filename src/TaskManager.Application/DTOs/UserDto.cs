namespace TaskManager.Application.DTOs;

/// <summary>
/// Data transfer object for user information.
/// </summary>
public record UserDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    DateTime CreatedAt);
