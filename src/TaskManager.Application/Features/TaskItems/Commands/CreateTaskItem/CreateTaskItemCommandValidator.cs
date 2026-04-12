using FluentValidation;

namespace TaskManager.Application.Features.TaskItems.Commands.CreateTaskItem;

/// <summary>
/// Validator for the <see cref="CreateTaskItemCommand"/>.
/// </summary>
public class CreateTaskItemCommandValidator : AbstractValidator<CreateTaskItemCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateTaskItemCommandValidator"/> class.
    /// </summary>
    public CreateTaskItemCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Task title is required.")
            .MaximumLength(200).WithMessage("Task title must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(5000).WithMessage("Task description must not exceed 5000 characters.");

        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("Project Id is required.");

        RuleFor(x => x.Priority)
            .IsInEnum().WithMessage("Invalid task priority.");
    }
}
