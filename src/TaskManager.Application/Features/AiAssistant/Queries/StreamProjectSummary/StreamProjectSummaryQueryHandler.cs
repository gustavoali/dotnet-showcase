using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskManager.Application.Common.Exceptions;
using TaskManager.Application.Common.Interfaces;
using TaskManager.Application.DTOs;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.Features.AiAssistant.Queries.StreamProjectSummary;

/// <summary>
/// Handles building the project summary context from persistence and delegating the streamed
/// summary generation to the AI assistant. Persistence stays in this layer; the AI call is decoupled.
/// </summary>
public class StreamProjectSummaryQueryHandler : IRequestHandler<StreamProjectSummaryQuery, IAsyncEnumerable<string>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAiAssistant _aiAssistant;

    /// <summary>
    /// Initializes a new instance of the <see cref="StreamProjectSummaryQueryHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">The unit of work.</param>
    /// <param name="aiAssistant">The AI assistant abstraction.</param>
    public StreamProjectSummaryQueryHandler(IUnitOfWork unitOfWork, IAiAssistant aiAssistant)
    {
        _unitOfWork = unitOfWork;
        _aiAssistant = aiAssistant;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// The assistant availability is checked eagerly here — before the streamed enumerable is
    /// returned to the controller — so that the not-configured case throws inside
    /// <c>await mediator.Send(...)</c> and the global exception handler can set HTTP 503 before any
    /// response headers are flushed. If the check were deferred to enumeration time (during response
    /// serialization), the 200 status and headers would already be on the wire.
    /// </remarks>
    public async Task<IAsyncEnumerable<string>> Handle(StreamProjectSummaryQuery request, CancellationToken cancellationToken)
    {
        if (!_aiAssistant.IsAvailable)
        {
            throw new AiUnavailableException(
                "The AI assistant is not configured. Set the ANTHROPIC_API_KEY environment variable to enable it.");
        }

        var project = await _unitOfWork.Projects.Query()
            .Include(p => p.Tasks)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId, cancellationToken);

        if (project is null)
        {
            throw new NotFoundException(nameof(Project), request.ProjectId);
        }

        var tasks = project.Tasks
            .Select(t => new ProjectSummaryTask(t.Title, t.Status, t.Priority, t.DueDate))
            .ToList();

        var context = new ProjectSummaryContext(project.Name, project.Description, tasks);

        return _aiAssistant.StreamSummaryAsync(context, cancellationToken);
    }
}
