namespace TaskManager.Domain.Enums;

/// <summary>
/// Represents the status of a project.
/// </summary>
public enum ProjectStatus
{
    /// <summary>Project is currently active.</summary>
    Active = 0,

    /// <summary>Project has been completed.</summary>
    Completed = 1,

    /// <summary>Project has been archived.</summary>
    Archived = 2
}
