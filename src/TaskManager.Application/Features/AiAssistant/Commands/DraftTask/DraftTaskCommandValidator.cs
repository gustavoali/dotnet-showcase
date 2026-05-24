using FluentValidation;

namespace TaskManager.Application.Features.AiAssistant.Commands.DraftTask;

/// <summary>
/// Validator for the <see cref="DraftTaskCommand"/>.
/// </summary>
public class DraftTaskCommandValidator : AbstractValidator<DraftTaskCommand>
{
    /// <summary>
    /// The maximum allowed length, in characters, for the natural-language input.
    /// </summary>
    public const int MaxInputLength = 2000;

    /// <summary>
    /// Initializes a new instance of the <see cref="DraftTaskCommandValidator"/> class.
    /// </summary>
    public DraftTaskCommandValidator()
    {
        RuleFor(x => x.Input)
            .NotEmpty().WithMessage("Input is required.")
            .MaximumLength(MaxInputLength).WithMessage($"Input must not exceed {MaxInputLength} characters.");
    }
}
