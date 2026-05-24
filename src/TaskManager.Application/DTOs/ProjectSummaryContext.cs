using TaskManager.Domain.Enums;

namespace TaskManager.Application.DTOs;

/// <summary>
/// Persistence-agnostic context describing a project and its tasks, supplied to the AI assistant
/// so it can stream a natural-language summary without depending on the data layer.
/// </summary>
/// <param name="ProjectName">The name of the project being summarized.</param>
/// <param name="ProjectDescription">The description of the project being summarized.</param>
/// <param name="Tasks">A read-only snapshot of the project's tasks.</param>
public record ProjectSummaryContext(
    string ProjectName,
    string ProjectDescription,
    IReadOnlyList<ProjectSummaryTask> Tasks);

/// <summary>
/// A lightweight, persistence-agnostic snapshot of a single task used to build the AI summary prompt.
/// </summary>
/// <param name="Title">The task title.</param>
/// <param name="Status">The task status.</param>
/// <param name="Priority">The task priority.</param>
/// <param name="DueDate">The task due date, if any; otherwise <see langword="null"/>.</param>
public record ProjectSummaryTask(
    string Title,
    TaskItemStatus Status,
    TaskPriority Priority,
    DateTime? DueDate);
