using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.Common;
using TaskManager.Application.DTOs;
using TaskManager.Application.Features.Comments.Commands.CreateComment;
using TaskManager.Application.Features.Comments.Queries.GetCommentsByTaskItem;
using TaskManager.Application.Features.TaskItems.Commands.DeleteTaskItem;
using TaskManager.Application.Features.TaskItems.Commands.UpdateTaskItem;
using TaskManager.Application.Features.TaskItems.Commands.UpdateTaskItemStatus;
using TaskManager.Application.Features.TaskItems.Queries.GetTaskItemById;

namespace TaskManager.API.Controllers;

/// <summary>
/// Controller for managing task items.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class TaskItemsController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="TaskItemsController"/> class.
    /// </summary>
    /// <param name="mediator">The MediatR mediator.</param>
    public TaskItemsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Gets a task item by its identifier.
    /// </summary>
    /// <param name="id">The task item identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The task item details.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TaskItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTaskItemById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetTaskItemByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Updates an existing task item.
    /// </summary>
    /// <param name="id">The task item identifier.</param>
    /// <param name="command">The update task item command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated task item.</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(TaskItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTaskItem(
        Guid id,
        [FromBody] UpdateTaskItemCommand command,
        CancellationToken cancellationToken = default)
    {
        if (id != command.Id)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Id mismatch.",
                Detail = "The route id does not match the command id."
            });
        }

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Updates the status of a task item.
    /// </summary>
    /// <param name="id">The task item identifier.</param>
    /// <param name="command">The update status command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated task item.</returns>
    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(typeof(TaskItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTaskItemStatus(
        Guid id,
        [FromBody] UpdateTaskItemStatusCommand command,
        CancellationToken cancellationToken = default)
    {
        if (id != command.Id)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Id mismatch.",
                Detail = "The route id does not match the command id."
            });
        }

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Deletes a task item.
    /// </summary>
    /// <param name="id">The task item identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTaskItem(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        await _mediator.Send(new DeleteTaskItemCommand(id), cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Creates a new comment on a task item.
    /// </summary>
    /// <param name="id">The task item identifier.</param>
    /// <param name="command">The create comment command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created comment.</returns>
    [HttpPost("{id:guid}/comments")]
    [ProducesResponseType(typeof(CommentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateComment(
        Guid id,
        [FromBody] CreateCommentCommand command,
        CancellationToken cancellationToken = default)
    {
        if (id != command.TaskItemId)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Task item Id mismatch.",
                Detail = "The route id does not match the command taskItemId."
            });
        }

        var result = await _mediator.Send(command, cancellationToken);
        return Created(string.Empty, result);
    }

    /// <summary>
    /// Gets a paginated list of comments for a task item.
    /// </summary>
    /// <param name="id">The task item identifier.</param>
    /// <param name="pageNumber">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated list of comments.</returns>
    [HttpGet("{id:guid}/comments")]
    [ProducesResponseType(typeof(PagedResult<CommentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCommentsByTaskItem(
        Guid id,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetCommentsByTaskItemQuery(id, pageNumber, pageSize);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}
