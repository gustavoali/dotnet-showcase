using FluentValidation;

namespace TaskManager.Application.Features.TaskItems.Commands.UpdateTaskItem;

/// <summary>
/// Validator for the <see cref="UpdateTaskItemCommand"/>.
/// </summary>
public class UpdateTaskItemCommandValidator : AbstractValidator<UpdateTaskItemCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateTaskItemCommandValidator"/> class.
    /// </summary>
    public UpdateTaskItemCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Task item Id is required.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Task title is required.")
            .MaximumLength(200).WithMessage("Task title must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(5000).WithMessage("Task description must not exceed 5000 characters.");

        RuleFor(x => x.Priority)
            .IsInEnum().WithMessage("Invalid task priority.");
    }
}
