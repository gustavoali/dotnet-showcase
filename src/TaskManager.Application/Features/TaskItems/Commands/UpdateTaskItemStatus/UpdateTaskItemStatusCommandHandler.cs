using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskManager.Application.Common.Exceptions;
using TaskManager.Application.DTOs;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.Features.TaskItems.Commands.UpdateTaskItemStatus;

/// <summary>
/// Handles updating the status of a task item.
/// </summary>
public class UpdateTaskItemStatusCommandHandler : IRequestHandler<UpdateTaskItemStatusCommand, TaskItemDto>
{
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateTaskItemStatusCommandHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">The unit of work.</param>
    public UpdateTaskItemStatusCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc/>
    public async Task<TaskItemDto> Handle(UpdateTaskItemStatusCommand request, CancellationToken cancellationToken)
    {
        var taskItem = await _unitOfWork.TaskItems.Query()
            .Include(t => t.Assignee)
            .Include(t => t.Comments)
            .Include(t => t.Tags)
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (taskItem is null)
        {
            throw new NotFoundException(nameof(TaskItem), request.Id);
        }

        taskItem.Status = request.Status;
        taskItem.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.TaskItems.Update(taskItem);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return taskItem.Adapt<TaskItemDto>();
    }
}
