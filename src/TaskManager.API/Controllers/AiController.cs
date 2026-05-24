using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.DTOs;
using TaskManager.Application.Features.AiAssistant.Commands.DraftTask;
using TaskManager.Application.Features.AiAssistant.Queries.StreamProjectSummary;

namespace TaskManager.API.Controllers;

/// <summary>
/// Controller exposing AI-assisted task management features.
/// </summary>
[ApiController]
[Route("api/ai")]
[Produces("application/json")]
[Authorize]
public class AiController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="AiController"/> class.
    /// </summary>
    /// <param name="mediator">The MediatR mediator.</param>
    public AiController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Drafts a structured task suggestion from natural-language input.
    /// </summary>
    /// <param name="command">The draft task command containing the natural-language input.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The structured task draft.</returns>
    [HttpPost("tasks/draft")]
    [ProducesResponseType(typeof(TaskDraftDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status502BadGateway)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> DraftTask(
        [FromBody] DraftTaskCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Streams a natural-language summary of a project's task status.
    /// </summary>
    /// <param name="id">The project identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// An asynchronous stream of summary text chunks. MVC serializes the stream incrementally as a
    /// JSON array of strings (<c>application/json</c>).
    /// </returns>
    /// <remarks>
    /// When the AI assistant is not configured this returns <c>503 Service Unavailable</c>; the check
    /// is performed eagerly by the handler so the status is set before any response body is flushed.
    /// </remarks>
    [HttpPost("projects/{id:guid}/summary")]
    [ProducesResponseType(typeof(IAsyncEnumerable<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IAsyncEnumerable<string>> StreamProjectSummary(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _mediator.Send(new StreamProjectSummaryQuery(id), cancellationToken);
    }
}
