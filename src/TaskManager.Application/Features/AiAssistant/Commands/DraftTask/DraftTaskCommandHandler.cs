using MediatR;
using TaskManager.Application.Common.Interfaces;
using TaskManager.Application.DTOs;

namespace TaskManager.Application.Features.AiAssistant.Commands.DraftTask;

/// <summary>
/// Handles drafting a structured task suggestion from natural-language input.
/// </summary>
public class DraftTaskCommandHandler : IRequestHandler<DraftTaskCommand, TaskDraftDto>
{
    private readonly IAiAssistant _aiAssistant;

    /// <summary>
    /// Initializes a new instance of the <see cref="DraftTaskCommandHandler"/> class.
    /// </summary>
    /// <param name="aiAssistant">The AI assistant abstraction.</param>
    public DraftTaskCommandHandler(IAiAssistant aiAssistant)
    {
        _aiAssistant = aiAssistant;
    }

    /// <inheritdoc/>
    public async Task<TaskDraftDto> Handle(DraftTaskCommand request, CancellationToken cancellationToken)
    {
        return await _aiAssistant.DraftTaskAsync(request.Input, cancellationToken);
    }
}
