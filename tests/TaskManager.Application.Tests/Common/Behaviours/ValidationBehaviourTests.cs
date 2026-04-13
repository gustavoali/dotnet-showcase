using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Moq;
using TaskManager.Application.Common.Behaviours;
using ValidationException = TaskManager.Application.Common.Exceptions.ValidationException;

namespace TaskManager.Application.Tests.Common.Behaviours;

public class ValidationBehaviourTests
{
    public record TestRequest(string Name) : IRequest<TestResponse>;
    public record TestResponse(string Result);

    [Fact]
    public async Task Handle_Should_ThrowValidationException_WhenValidationFails()
    {
        // Arrange
        var failures = new List<ValidationFailure>
        {
            new("Name", "Name is required.")
        };

        var validatorMock = new Mock<IValidator<TestRequest>>();
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(failures));

        var validators = new List<IValidator<TestRequest>> { validatorMock.Object };
        var behaviour = new ValidationBehaviour<TestRequest, TestResponse>(validators);

        var request = new TestRequest("");
        var nextCalled = false;

        RequestHandlerDelegate<TestResponse> next = delegate
        {
            nextCalled = true;
            return Task.FromResult(new TestResponse("OK"));
        };

        // Act
        var act = () => behaviour.Handle(request, next, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
        nextCalled.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_Should_CallNext_WhenNoValidators()
    {
        // Arrange
        var validators = Enumerable.Empty<IValidator<TestRequest>>();
        var behaviour = new ValidationBehaviour<TestRequest, TestResponse>(validators);

        var expectedResponse = new TestResponse("OK");
        var nextCalled = false;
        RequestHandlerDelegate<TestResponse> next = delegate
        {
            nextCalled = true;
            return Task.FromResult(expectedResponse);
        };

        var request = new TestRequest("Valid");

        // Act
        var result = await behaviour.Handle(request, next, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResponse);
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_Should_CallNext_WhenValidationPasses()
    {
        // Arrange
        var validatorMock = new Mock<IValidator<TestRequest>>();
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var validators = new List<IValidator<TestRequest>> { validatorMock.Object };
        var behaviour = new ValidationBehaviour<TestRequest, TestResponse>(validators);

        var expectedResponse = new TestResponse("OK");
        var nextCalled = false;
        RequestHandlerDelegate<TestResponse> next = delegate
        {
            nextCalled = true;
            return Task.FromResult(expectedResponse);
        };

        var request = new TestRequest("Valid");

        // Act
        var result = await behaviour.Handle(request, next, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResponse);
        nextCalled.Should().BeTrue();
    }
}
