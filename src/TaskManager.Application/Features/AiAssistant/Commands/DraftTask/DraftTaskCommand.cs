using MediatR;
using TaskManager.Application.DTOs;

namespace TaskManager.Application.Features.AiAssistant.Commands.DraftTask;

/// <summary>
/// Command to draft a structured task suggestion from natural-language input using the AI assistant.
/// </summary>
/// <param name="Input">The natural-language description of the task to draft.</param>
public record DraftTaskCommand(string Input) : IRequest<TaskDraftDto>;
