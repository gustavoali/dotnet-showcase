using System.Reflection;
using Microsoft.EntityFrameworkCore;
using TaskManager.Domain.Entities;

namespace TaskManager.Infrastructure.Persistence;

/// <summary>
/// The main database context for the application.
/// </summary>
public class ApplicationDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationDbContext"/> class.
    /// </summary>
    /// <param name="options">The database context options.</param>
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    /// <summary>Gets or sets the Users DbSet.</summary>
    public DbSet<User> Users => Set<User>();

    /// <summary>Gets or sets the Projects DbSet.</summary>
    public DbSet<Project> Projects => Set<Project>();

    /// <summary>Gets or sets the TaskItems DbSet.</summary>
    public DbSet<TaskItem> TaskItems => Set<TaskItem>();

    /// <summary>Gets or sets the Comments DbSet.</summary>
    public DbSet<Comment> Comments => Set<Comment>();

    /// <summary>Gets or sets the Tags DbSet.</summary>
    public DbSet<Tag> Tags => Set<Tag>();

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
