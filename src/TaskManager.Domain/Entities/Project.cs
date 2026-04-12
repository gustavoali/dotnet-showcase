using TaskManager.Domain.Enums;

namespace TaskManager.Domain.Entities;

/// <summary>
/// Represents a project that groups related tasks.
/// </summary>
public class Project : BaseEntity
{
    /// <summary>
    /// Gets or sets the project name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the project description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the owner's unique identifier.
    /// </summary>
    public Guid OwnerId { get; set; }

    /// <summary>
    /// Gets or sets the project status.
    /// </summary>
    public ProjectStatus Status { get; set; } = ProjectStatus.Active;

    /// <summary>
    /// Gets or sets the project owner.
    /// </summary>
    public User Owner { get; set; } = null!;

    /// <summary>
    /// Gets or sets the collection of tasks belonging to this project.
    /// </summary>
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}
