using TaskManager.Domain.Enums;

namespace TaskManager.Application.DTOs;

/// <summary>
/// Data transfer object for project information.
/// </summary>
public record ProjectDto(
    Guid Id,
    string Name,
    string Description,
    Guid OwnerId,
    string OwnerName,
    ProjectStatus Status,
    int TaskCount,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
