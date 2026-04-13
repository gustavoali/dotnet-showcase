using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

namespace TaskManager.Application.Tests.Helpers;

/// <summary>
/// Provides an in-memory IQueryable that supports EF Core async operations
/// (FirstOrDefaultAsync, ToListAsync, CountAsync, etc.) for unit testing.
/// </summary>
internal static class AsyncQueryableExtensions
{
    /// <summary>
    /// Wraps an IQueryable with an async-capable provider for EF Core testing.
    /// </summary>
    public static IQueryable<T> AsAsyncQueryable<T>(this IEnumerable<T> source)
    {
        return new TestAsyncEnumerable<T>(source.AsQueryable());
    }
}

internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
{
    private readonly IQueryable<T> _inner;

    public TestAsyncEnumerable(IQueryable<T> inner) : base(inner.Expression)
    {
        _inner = inner;
    }

    IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(_inner.Provider);

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        return new TestAsyncEnumerator<T>(_inner.GetEnumerator());
    }
}

internal class TestAsyncQueryProvider<T> : IAsyncQueryProvider
{
    private readonly IQueryProvider _inner;

    public TestAsyncQueryProvider(IQueryProvider inner)
    {
        _inner = inner;
    }

    public IQueryable CreateQuery(Expression expression) => new TestAsyncEnumerable<T>(_inner.CreateQuery<T>(expression));
    public IQueryable<TElement> CreateQuery<TElement>(Expression expression) => new TestAsyncEnumerable<TElement>(_inner.CreateQuery<TElement>(expression));
    public object? Execute(Expression expression) => _inner.Execute(expression);
    public TResult Execute<TResult>(Expression expression) => _inner.Execute<TResult>(expression);

    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
    {
        var resultType = typeof(TResult).GetGenericArguments().FirstOrDefault() ?? typeof(TResult);
        var executeMethod = typeof(IQueryProvider).GetMethod(nameof(IQueryProvider.Execute), 1, new[] { typeof(Expression) })!;
        var result = executeMethod.MakeGenericMethod(resultType).Invoke(_inner, new object[] { expression });
        return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))!.MakeGenericMethod(resultType).Invoke(null, new[] { result })!;
    }
}

internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _inner;

    public TestAsyncEnumerator(IEnumerator<T> inner) => _inner = inner;
    public T Current => _inner.Current;
    public ValueTask DisposeAsync() { _inner.Dispose(); return ValueTask.CompletedTask; }
    public ValueTask<bool> MoveNextAsync() => new(_inner.MoveNext());
}
