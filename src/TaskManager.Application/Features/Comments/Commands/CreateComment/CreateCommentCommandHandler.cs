using Mapster;
using MediatR;
using TaskManager.Application.Common.Exceptions;
using TaskManager.Application.Common.Interfaces;
using TaskManager.Application.DTOs;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.Features.Comments.Commands.CreateComment;

/// <summary>
/// Handles the creation of a new comment on a task item.
/// </summary>
public class CreateCommentCommandHandler : IRequestHandler<CreateCommentCommand, CommentDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateCommentCommandHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">The unit of work.</param>
    /// <param name="currentUserService">The current user service.</param>
    public CreateCommentCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    /// <inheritdoc/>
    public async Task<CommentDto> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
    {
        var taskItem = await _unitOfWork.TaskItems.GetByIdAsync(request.TaskItemId, cancellationToken);
        if (taskItem is null)
        {
            throw new NotFoundException(nameof(TaskItem), request.TaskItemId);
        }

        var comment = new Comment
        {
            Id = Guid.NewGuid(),
            TaskItemId = request.TaskItemId,
            AuthorId = _currentUserService.UserId!.Value,
            Content = request.Content,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Comments.AddAsync(comment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Return with author name
        var user = await _unitOfWork.Users.GetByIdAsync(comment.AuthorId, cancellationToken);
        var authorName = user is not null ? $"{user.FirstName} {user.LastName}" : string.Empty;

        return new CommentDto(
            comment.Id,
            comment.TaskItemId,
            comment.AuthorId,
            authorName,
            comment.Content,
            comment.CreatedAt);
    }
}
