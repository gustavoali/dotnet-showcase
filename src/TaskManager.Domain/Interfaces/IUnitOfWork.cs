using TaskManager.Domain.Entities;

namespace TaskManager.Domain.Interfaces;

/// <summary>
/// Unit of Work interface for managing transactions across repositories.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Gets the repository for <see cref="Project"/> entities.
    /// </summary>
    IRepository<Project> Projects { get; }

    /// <summary>
    /// Gets the repository for <see cref="TaskItem"/> entities.
    /// </summary>
    IRepository<TaskItem> TaskItems { get; }

    /// <summary>
    /// Gets the repository for <see cref="Comment"/> entities.
    /// </summary>
    IRepository<Comment> Comments { get; }

    /// <summary>
    /// Gets the repository for <see cref="Tag"/> entities.
    /// </summary>
    IRepository<Tag> Tags { get; }

    /// <summary>
    /// Gets the repository for <see cref="User"/> entities.
    /// </summary>
    IRepository<User> Users { get; }

    /// <summary>
    /// Saves all pending changes to the database.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The number of state entries written to the database.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
