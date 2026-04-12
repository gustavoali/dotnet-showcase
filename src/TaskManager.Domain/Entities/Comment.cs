namespace TaskManager.Domain.Entities;

/// <summary>
/// Represents a comment on a task item.
/// </summary>
public class Comment : BaseEntity
{
    /// <summary>
    /// Gets or sets the parent task item's unique identifier.
    /// </summary>
    public Guid TaskItemId { get; set; }

    /// <summary>
    /// Gets or sets the author's unique identifier.
    /// </summary>
    public Guid AuthorId { get; set; }

    /// <summary>
    /// Gets or sets the comment content.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the parent task item.
    /// </summary>
    public TaskItem TaskItem { get; set; } = null!;

    /// <summary>
    /// Gets or sets the comment author.
    /// </summary>
    public User Author { get; set; } = null!;
}
