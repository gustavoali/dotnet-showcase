using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;
using TaskManager.Infrastructure.Persistence.Repositories;

namespace TaskManager.Infrastructure.Persistence;

/// <summary>
/// Unit of Work implementation for managing database transactions.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IRepository<Project>? _projects;
    private IRepository<TaskItem>? _taskItems;
    private IRepository<Comment>? _comments;
    private IRepository<Tag>? _tags;
    private IRepository<User>? _users;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="UnitOfWork"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public IRepository<Project> Projects =>
        _projects ??= new GenericRepository<Project>(_context);

    /// <inheritdoc/>
    public IRepository<TaskItem> TaskItems =>
        _taskItems ??= new GenericRepository<TaskItem>(_context);

    /// <inheritdoc/>
    public IRepository<Comment> Comments =>
        _comments ??= new GenericRepository<Comment>(_context);

    /// <inheritdoc/>
    public IRepository<Tag> Tags =>
        _tags ??= new GenericRepository<Tag>(_context);

    /// <inheritdoc/>
    public IRepository<User> Users =>
        _users ??= new GenericRepository<User>(_context);

    /// <inheritdoc/>
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the managed resources.
    /// </summary>
    /// <param name="disposing">True if disposing managed resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _context.Dispose();
        }
        _disposed = true;
    }
}
