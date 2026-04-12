using MediatR;
using TaskManager.Application.Common;
using TaskManager.Application.DTOs;

namespace TaskManager.Application.Features.Comments.Queries.GetCommentsByTaskItem;

/// <summary>
/// Query to retrieve a paginated list of comments for a specific task item.
/// </summary>
public record GetCommentsByTaskItemQuery(
    Guid TaskItemId,
    int PageNumber = 1,
    int PageSize = 10) : IRequest<PagedResult<CommentDto>>;
