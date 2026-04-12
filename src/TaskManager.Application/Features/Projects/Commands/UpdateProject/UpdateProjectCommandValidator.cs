using FluentValidation;

namespace TaskManager.Application.Features.Projects.Commands.UpdateProject;

/// <summary>
/// Validator for the <see cref="UpdateProjectCommand"/>.
/// </summary>
public class UpdateProjectCommandValidator : AbstractValidator<UpdateProjectCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateProjectCommandValidator"/> class.
    /// </summary>
    public UpdateProjectCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Project Id is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Project name is required.")
            .MaximumLength(200).WithMessage("Project name must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Project description must not exceed 2000 characters.");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid project status.");
    }
}
