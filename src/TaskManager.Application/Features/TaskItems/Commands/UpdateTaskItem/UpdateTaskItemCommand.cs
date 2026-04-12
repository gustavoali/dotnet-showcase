using MediatR;
using TaskManager.Application.DTOs;
using TaskManager.Domain.Enums;

namespace TaskManager.Application.Features.TaskItems.Commands.UpdateTaskItem;

/// <summary>
/// Command to update an existing task item.
/// </summary>
public record UpdateTaskItemCommand(
    Guid Id,
    string Title,
    string Description,
    Guid? AssigneeId,
    TaskPriority Priority,
    DateTime? DueDate) : IRequest<TaskItemDto>;
