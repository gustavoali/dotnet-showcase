using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using TaskManager.API.Middleware;
using TaskManager.Application.Common.Exceptions;

namespace TaskManager.API.Tests.Middleware;

/// <summary>
/// Unit tests for <see cref="GlobalExceptionHandlerMiddleware"/>.
/// </summary>
public class GlobalExceptionHandlerMiddlewareTests
{
    private readonly Mock<ILogger<GlobalExceptionHandlerMiddleware>> _loggerMock;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public GlobalExceptionHandlerMiddlewareTests()
    {
        _loggerMock = new Mock<ILogger<GlobalExceptionHandlerMiddleware>>();
    }

    [Fact]
    public async Task InvokeAsync_Should_Return404_ForNotFoundException()
    {
        // Arrange
        RequestDelegate next = _ => throw new NotFoundException("Project", Guid.NewGuid());
        var middleware = new GlobalExceptionHandlerMiddleware(next, _loggerMock.Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        context.Response.ContentType.Should().Be("application/problem+json");

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(body, _jsonOptions);

        problemDetails.Should().NotBeNull();
        problemDetails!.Status.Should().Be(404);
    }

    [Fact]
    public async Task InvokeAsync_Should_Return400_ForValidationException()
    {
        // Arrange
        var failures = new List<FluentValidation.Results.ValidationFailure>
        {
            new("Name", "Name is required.")
        };
        RequestDelegate next = _ => throw new ValidationException(failures);
        var middleware = new GlobalExceptionHandlerMiddleware(next, _loggerMock.Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        body.Should().Contain("Name");
    }

    [Fact]
    public async Task InvokeAsync_Should_Return403_ForForbiddenAccessException()
    {
        // Arrange
        RequestDelegate next = _ => throw new ForbiddenAccessException("Access denied.");
        var middleware = new GlobalExceptionHandlerMiddleware(next, _loggerMock.Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.Forbidden);
        context.Response.ContentType.Should().Be("application/problem+json");
    }

    [Fact]
    public async Task InvokeAsync_Should_Return500_ForUnhandledException()
    {
        // Arrange
        RequestDelegate next = _ => throw new InvalidOperationException("Something went wrong");
        var middleware = new GlobalExceptionHandlerMiddleware(next, _loggerMock.Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
        context.Response.ContentType.Should().Be("application/problem+json");
    }
}
