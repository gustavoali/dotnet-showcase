using FluentValidation;

namespace TaskManager.Application.Features.Comments.Commands.CreateComment;

/// <summary>
/// Validator for the <see cref="CreateCommentCommand"/>.
/// </summary>
public class CreateCommentCommandValidator : AbstractValidator<CreateCommentCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateCommentCommandValidator"/> class.
    /// </summary>
    public CreateCommentCommandValidator()
    {
        RuleFor(x => x.TaskItemId)
            .NotEmpty().WithMessage("Task item Id is required.");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Comment content is required.")
            .MaximumLength(5000).WithMessage("Comment content must not exceed 5000 characters.");
    }
}
