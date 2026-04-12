using MediatR;
using TaskManager.Application.DTOs;

namespace TaskManager.Application.Features.TaskItems.Queries.GetTaskItemById;

/// <summary>
/// Query to retrieve a single task item by its identifier.
/// </summary>
public record GetTaskItemByIdQuery(Guid Id) : IRequest<TaskItemDto>;
