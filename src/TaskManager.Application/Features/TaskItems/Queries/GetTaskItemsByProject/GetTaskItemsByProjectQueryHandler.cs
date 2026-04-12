using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskManager.Application.Common;
using TaskManager.Application.DTOs;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.Features.TaskItems.Queries.GetTaskItemsByProject;

/// <summary>
/// Handles retrieving a paginated list of task items for a specific project.
/// </summary>
public class GetTaskItemsByProjectQueryHandler : IRequestHandler<GetTaskItemsByProjectQuery, PagedResult<TaskItemDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetTaskItemsByProjectQueryHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">The unit of work.</param>
    public GetTaskItemsByProjectQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc/>
    public async Task<PagedResult<TaskItemDto>> Handle(GetTaskItemsByProjectQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.TaskItems.Query()
            .Include(t => t.Assignee)
            .Include(t => t.Comments)
            .Include(t => t.Tags)
            .Where(t => t.ProjectId == request.ProjectId)
            .AsNoTracking();

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<TaskItemDto>
        {
            Items = items.Adapt<List<TaskItemDto>>(),
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
