namespace TaskManager.Domain.Enums;

/// <summary>
/// Represents the status of a task item.
/// </summary>
public enum TaskItemStatus
{
    /// <summary>Task is pending and not yet started.</summary>
    Todo = 0,

    /// <summary>Task is currently being worked on.</summary>
    InProgress = 1,

    /// <summary>Task has been completed.</summary>
    Done = 2,

    /// <summary>Task has been cancelled.</summary>
    Cancelled = 3
}
