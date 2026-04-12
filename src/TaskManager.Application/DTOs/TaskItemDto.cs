using TaskManager.Domain.Enums;

namespace TaskManager.Application.DTOs;

/// <summary>
/// Data transfer object for task item information.
/// </summary>
public record TaskItemDto(
    Guid Id,
    string Title,
    string Description,
    Guid ProjectId,
    Guid? AssigneeId,
    string? AssigneeName,
    TaskItemStatus Status,
    TaskPriority Priority,
    DateTime? DueDate,
    int CommentCount,
    IReadOnlyList<string> Tags,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
