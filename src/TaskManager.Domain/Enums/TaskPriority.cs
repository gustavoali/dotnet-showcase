namespace TaskManager.Domain.Enums;

/// <summary>
/// Represents the priority level of a task item.
/// </summary>
public enum TaskPriority
{
    /// <summary>Low priority.</summary>
    Low = 0,

    /// <summary>Medium priority.</summary>
    Medium = 1,

    /// <summary>High priority.</summary>
    High = 2,

    /// <summary>Critical priority requiring immediate attention.</summary>
    Critical = 3
}
