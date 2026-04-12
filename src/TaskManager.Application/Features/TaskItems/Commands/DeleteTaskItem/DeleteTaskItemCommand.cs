using MediatR;

namespace TaskManager.Application.Features.TaskItems.Commands.DeleteTaskItem;

/// <summary>
/// Command to delete an existing task item.
/// </summary>
public record DeleteTaskItemCommand(Guid Id) : IRequest<Unit>;
