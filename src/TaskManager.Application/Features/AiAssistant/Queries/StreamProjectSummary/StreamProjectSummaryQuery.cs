using MediatR;

namespace TaskManager.Application.Features.AiAssistant.Queries.StreamProjectSummary;

/// <summary>
/// Query that produces a streamed natural-language summary of a project's task status.
/// </summary>
/// <param name="ProjectId">The identifier of the project to summarize.</param>
public record StreamProjectSummaryQuery(Guid ProjectId) : IRequest<IAsyncEnumerable<string>>;
