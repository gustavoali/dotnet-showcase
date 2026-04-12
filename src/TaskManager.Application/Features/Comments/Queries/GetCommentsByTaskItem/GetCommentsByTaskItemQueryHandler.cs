using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskManager.Application.Common;
using TaskManager.Application.DTOs;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.Features.Comments.Queries.GetCommentsByTaskItem;

/// <summary>
/// Handles retrieving a paginated list of comments for a specific task item.
/// </summary>
public class GetCommentsByTaskItemQueryHandler : IRequestHandler<GetCommentsByTaskItemQuery, PagedResult<CommentDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetCommentsByTaskItemQueryHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">The unit of work.</param>
    public GetCommentsByTaskItemQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc/>
    public async Task<PagedResult<CommentDto>> Handle(GetCommentsByTaskItemQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Comments.Query()
            .Include(c => c.Author)
            .Where(c => c.TaskItemId == request.TaskItemId)
            .AsNoTracking();

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<CommentDto>
        {
            Items = items.Adapt<List<CommentDto>>(),
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
