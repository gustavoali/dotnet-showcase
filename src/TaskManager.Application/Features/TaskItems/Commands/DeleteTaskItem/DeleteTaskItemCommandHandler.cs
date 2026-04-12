using MediatR;
using TaskManager.Application.Common.Exceptions;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.Features.TaskItems.Commands.DeleteTaskItem;

/// <summary>
/// Handles deleting an existing task item.
/// </summary>
public class DeleteTaskItemCommandHandler : IRequestHandler<DeleteTaskItemCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteTaskItemCommandHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">The unit of work.</param>
    public DeleteTaskItemCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc/>
    public async Task<Unit> Handle(DeleteTaskItemCommand request, CancellationToken cancellationToken)
    {
        var taskItem = await _unitOfWork.TaskItems.GetByIdAsync(request.Id, cancellationToken);

        if (taskItem is null)
        {
            throw new NotFoundException(nameof(TaskItem), request.Id);
        }

        _unitOfWork.TaskItems.Delete(taskItem);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
