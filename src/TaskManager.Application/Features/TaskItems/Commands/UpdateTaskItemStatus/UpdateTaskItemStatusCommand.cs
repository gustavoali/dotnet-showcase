using MediatR;
using TaskManager.Application.DTOs;
using TaskManager.Domain.Enums;

namespace TaskManager.Application.Features.TaskItems.Commands.UpdateTaskItemStatus;

/// <summary>
/// Command to update only the status of a task item.
/// </summary>
public record UpdateTaskItemStatusCommand(
    Guid Id,
    TaskItemStatus Status) : IRequest<TaskItemDto>;
