using TaskManager.Domain.Enums;

namespace TaskManager.Domain.Entities;

/// <summary>
/// Represents a task item within a project.
/// </summary>
public class TaskItem : BaseEntity
{
    /// <summary>
    /// Gets or sets the task title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the task description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the parent project's unique identifier.
    /// </summary>
    public Guid ProjectId { get; set; }

    /// <summary>
    /// Gets or sets the assignee's unique identifier.
    /// </summary>
    public Guid? AssigneeId { get; set; }

    /// <summary>
    /// Gets or sets the task status.
    /// </summary>
    public TaskItemStatus Status { get; set; } = TaskItemStatus.Todo;

    /// <summary>
    /// Gets or sets the task priority.
    /// </summary>
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    /// <summary>
    /// Gets or sets the due date for the task.
    /// </summary>
    public DateTime? DueDate { get; set; }

    /// <summary>
    /// Gets or sets the parent project.
    /// </summary>
    public Project Project { get; set; } = null!;

    /// <summary>
    /// Gets or sets the assigned user.
    /// </summary>
    public User? Assignee { get; set; }

    /// <summary>
    /// Gets or sets the collection of comments on this task.
    /// </summary>
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();

    /// <summary>
    /// Gets or sets the collection of tags associated with this task.
    /// </summary>
    public ICollection<Tag> Tags { get; set; } = new List<Tag>();
}
