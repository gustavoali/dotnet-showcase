namespace TaskManager.Application.DTOs;

/// <summary>
/// Data transfer object for comment information.
/// </summary>
public record CommentDto(
    Guid Id,
    Guid TaskItemId,
    Guid AuthorId,
    string AuthorName,
    string Content,
    DateTime CreatedAt);
