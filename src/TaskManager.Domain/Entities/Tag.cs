namespace TaskManager.Domain.Entities;

/// <summary>
/// Represents a tag that can be applied to tasks.
/// </summary>
public class Tag : BaseEntity
{
    /// <summary>
    /// Gets or sets the tag name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the collection of tasks associated with this tag.
    /// </summary>
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}
