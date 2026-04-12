using MediatR;
using TaskManager.Application.DTOs;
using TaskManager.Domain.Enums;

namespace TaskManager.Application.Features.TaskItems.Commands.CreateTaskItem;

/// <summary>
/// Command to create a new task item within a project.
/// </summary>
public record CreateTaskItemCommand(
    string Title,
    string Description,
    Guid ProjectId,
    TaskPriority Priority,
    DateTime? DueDate) : IRequest<TaskItemDto>;
