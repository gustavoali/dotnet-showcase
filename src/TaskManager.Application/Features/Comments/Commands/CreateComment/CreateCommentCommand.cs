using MediatR;
using TaskManager.Application.DTOs;

namespace TaskManager.Application.Features.Comments.Commands.CreateComment;

/// <summary>
/// Command to create a new comment on a task item.
/// </summary>
public record CreateCommentCommand(
    Guid TaskItemId,
    string Content) : IRequest<CommentDto>;
