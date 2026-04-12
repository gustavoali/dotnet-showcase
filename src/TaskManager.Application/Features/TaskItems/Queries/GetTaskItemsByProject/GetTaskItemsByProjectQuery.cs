using MediatR;
using TaskManager.Application.Common;
using TaskManager.Application.DTOs;

namespace TaskManager.Application.Features.TaskItems.Queries.GetTaskItemsByProject;

/// <summary>
/// Query to retrieve a paginated list of task items for a specific project.
/// </summary>
public record GetTaskItemsByProjectQuery(
    Guid ProjectId,
    int PageNumber = 1,
    int PageSize = 10) : IRequest<PagedResult<TaskItemDto>>;
