using TaskManager.Domain.Enums;

namespace TaskManager.Application.DTOs;

/// <summary>
/// Data transfer object representing an AI-suggested task draft produced from natural-language input.
/// </summary>
/// <param name="Title">The suggested task title.</param>
/// <param name="Description">The suggested task description.</param>
/// <param name="Priority">The suggested task priority.</param>
/// <param name="DueDate">The suggested due date, if one could be inferred; otherwise <see langword="null"/>.</param>
public record TaskDraftDto(
    string Title,
    string Description,
    TaskPriority Priority,
    DateTime? DueDate);
