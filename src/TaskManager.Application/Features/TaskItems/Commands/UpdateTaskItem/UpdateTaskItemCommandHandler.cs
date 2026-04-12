using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskManager.Application.Common.Exceptions;
using TaskManager.Application.DTOs;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.Features.TaskItems.Commands.UpdateTaskItem;

/// <summary>
/// Handles updating an existing task item.
/// </summary>
public class UpdateTaskItemCommandHandler : IRequestHandler<UpdateTaskItemCommand, TaskItemDto>
{
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateTaskItemCommandHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">The unit of work.</param>
    public UpdateTaskItemCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc/>
    public async Task<TaskItemDto> Handle(UpdateTaskItemCommand request, CancellationToken cancellationToken)
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

        taskItem.Title = request.Title;
        taskItem.Description = request.Description;
        taskItem.AssigneeId = request.AssigneeId;
        taskItem.Priority = request.Priority;
        taskItem.DueDate = request.DueDate;
        taskItem.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.TaskItems.Update(taskItem);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return taskItem.Adapt<TaskItemDto>();
    }
}
