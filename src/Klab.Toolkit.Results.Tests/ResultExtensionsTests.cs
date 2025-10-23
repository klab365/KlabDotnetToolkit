using System;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Klab.Toolkit.Results.Tests;

public class ResultExtensionsTests
{
    [Fact]
    public void Map_ShouldTransformSuccessValue()
    {
        Result<int> result = Result.Success(42).Map(x => x * 2);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(84);
    }

    [Fact]
    public void Map_ShouldReturnFailureOnSourceFailure()
    {
        Result<int> result = Result.Failure<int>(Error.Create("ErrorCode", "Error")).Map(x => x * 2);

        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Be("Error");
    }

    [Fact]
    public void Bind_ShouldChainOperationsOnSuccess()
    {
        Result<int> result = Result.Success(42).Bind(x => Result.Success(x * 2));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(84);
    }

    [Fact]
    public void Bind_ShouldReturnFailureOnSourceFailure()
    {
        Result<int> result = Result.Failure<int>(Error.Create("ErrorCode", "Error")).Bind(x => Result.Success(x * 2));

        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Be("Error");
    }

    [Fact]
    public void Bind_ShouldReturnFailureOnBindFailure()
    {
        Result result = Result.Success(42).Bind(() => Result.Success());

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Bind_Multiple()
    {
        Result<int> result = Result.Success(42)
            .Bind(x => x > 0 ? Result.Success(x * 2) : Result.Failure<int>(Error.Create("ErrorCode", "ThresholdError1")))
            .Bind(x => x > 100 ? Result.Success(x * 2) : Result.Failure<int>(Error.Create("ErrorCode", "ThresholdError2")));

        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Be("ThresholdError2");
    }

    [Fact]
    public Task BindAsync_Multiple()
    {
        Result result = Result.Success(199)
            .Bind(x => x > 0 ? Result.Success(x * 2) : Result.Failure<int>(Error.Create("ErrorCode", "ThresholdError1")))
            .Bind(x => x > 100 ? Result.Success(x * 2) : Result.Failure<int>(Error.Create("ErrorCode", "ThresholdError2")))
            .Bind(x => x > 1000 ? Result.Success() : Result.Failure(Error.Create("ErrorCode", "ThresholdError3")));

        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Be("ThresholdError3");
        return Task.CompletedTask;
    }

    [Fact]
    public async Task BindAsync_FromResultSuccess_ShouldChainOperationsOnSuccess()
    {
        // Arrange
        Result initialResult = Result.Success();

        // Act
        Result result = await initialResult.BindAsync(async () =>
        {
            await Task.Delay(10);
            return Result.Success();
        });

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task BindAsync_FromResultSuccess_ShouldReturnFailureOnBindFailure()
    {
        // Arrange
        Result initialResult = Result.Success();
        Error error = Error.Create("ErrorCode", "Error");

        // Act
        Result result = await initialResult.BindAsync(async () =>
        {
            await Task.Delay(10);
            return Result.Failure(error);
        });

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public async Task BindAsync_FromResultFailure_ShouldNotExecuteBindFunc()
    {
        // Arrange
        Error initialError = Error.Create("ErrorCode", "Initial Error");
        Result initialResult = Result.Failure(initialError);
        bool bindFuncExecuted = false;

        // Act
        Result result = await initialResult.BindAsync(async () =>
        {
            bindFuncExecuted = true;
            await Task.Delay(10);
            return Result.Success();
        });

        // Assert
        bindFuncExecuted.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(initialError);
    }

    [Fact]
    public async Task BindAsync_FromResultSuccess_ShouldChainOperationsWithGenericValueOnSuccess()
    {
        // Arrange
        Result initialResult = Result.Success();
        string expectedValue = "Success Value";

        // Act
        Result<string> result = await initialResult.BindAsync(async () =>
        {
            await Task.Delay(10);
            return Result.Success(expectedValue);
        });

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedValue);
    }

    [Fact]
    public async Task BindAsync_FromResultSuccess_ShouldReturnFailureWithGenericValueOnBindFailure()
    {
        // Arrange
        Result initialResult = Result.Success();
        Error error = Error.Create("ErrorCode", "Error");

        // Act
        Result<string> result = await initialResult.BindAsync<string>(async () =>
        {
            await Task.Delay(10);
            return Result.Failure<string>(error);
        });

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public async Task BindAsync_FromResultFailure_ShouldNotExecuteBindFuncWithGenericValue()
    {
        // Arrange
        Error initialError = Error.Create("ErrorCode", "Initial Error");
        Result initialResult = Result.Failure(initialError);
        bool bindFuncExecuted = false;

        // Act
        Result<string> result = await initialResult.BindAsync<string>(async () =>
        {
            bindFuncExecuted = true;
            await Task.Delay(10);
            return Result.Success("Success Value");
        });

        // Assert
        bindFuncExecuted.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(initialError);
    }

    [Fact]
    public void OnSuccess_ShouldExecuteActionOnSuccess()
    {
        bool actionExecuted = false;
        Result.Success("Success").OnSuccess(_ => actionExecuted = true);

        actionExecuted.Should().BeTrue();
    }

    [Fact]
    public void OnSuccess_ShouldExecuteActionOnSuccessWithGenericValue()
    {
        string capturedValue = string.Empty;
        Result.Success("Success").OnSuccess(value => capturedValue = value);

        capturedValue.Should().Be("Success");
    }

    [Fact]
    public void OnFailure_ShouldExecuteActionOnFailure()
    {
        bool actionExecuted = false;
        Result.Failure<string>(Error.Create("ErrorCode", "Error")).OnFailure(_ => actionExecuted = true);

        actionExecuted.Should().BeTrue();
    }

    [Fact]
    public void ToResult_ShouldWrapValueInSuccessResult()
    {
        Result<int> result = 42.ToResult();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void Unwrap_ShouldRetrieveValueOnSuccess()
    {
        int value = Result.Success(42).Unwrap();

        value.Should().Be(42);
    }

    [Fact]
    public void Unwrap_ShouldThrowExceptionOnFailure()
    {
        Result<int> result = Result.Failure<int>(Error.Create("ErrorCode", "Error"));

        result.Invoking(r => r.Unwrap()).Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot unwrap a failure result. Error Code: ErrorCode, Message: Error");
    }

    [Fact]
    public void Unwrap_VoidResult_ShouldNotThrowOnSuccess()
    {
        Result result = Result.Success();

        result.Invoking(r => r.Unwrap()).Should().NotThrow();
    }

    [Fact]
    public void Unwrap_VoidResult_ShouldThrowExceptionOnFailure()
    {
        Result result = Result.Failure(Error.Create("ErrorCode", "Error"));

        result.Invoking(r => r.Unwrap()).Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot unwrap a failure result. Error Code: ErrorCode, Message: Error");
    }

    [Fact]
    public async Task UnwrapAsync_ShouldRetrieveValueOnSuccess()
    {
        Task<Result<int>> resultTask = Task.FromResult(Result.Success(42));

        int value = await resultTask.UnwrapAsync();

        value.Should().Be(42);
    }

    [Fact]
    public async Task UnwrapAsync_ShouldThrowExceptionOnFailure()
    {
        Task<Result<int>> resultTask = Task.FromResult(Result.Failure<int>(Error.Create("ErrorCode", "Error")));

        Func<Task> act = async () => await resultTask.UnwrapAsync();

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Cannot unwrap a failure result. Error Code: ErrorCode, Message: Error");
    }

    [Fact]
    public async Task UnwrapAsync_VoidResult_ShouldNotThrowOnSuccess()
    {
        Task<Result> resultTask = Task.FromResult(Result.Success());

        Func<Task> act = async () => await resultTask.UnwrapAsync();

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task UnwrapAsync_VoidResult_ShouldThrowExceptionOnFailure()
    {
        Task<Result> resultTask = Task.FromResult(Result.Failure(Error.Create("ErrorCode", "Error")));

        Func<Task> act = async () => await resultTask.UnwrapAsync();

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Cannot unwrap a failure result. Error Code: ErrorCode, Message: Error");
    }

    [Fact]
    public void UnwrapOr_ShouldRetrieveValueOnSuccess()
    {
        Result<int> result = Result.Success(42);

        int value = result.UnwrapOr(99);

        value.Should().Be(42);
    }

    [Fact]
    public void UnwrapOr_ShouldReturnDefaultOnFailure()
    {
        Result<int> result = Result.Failure<int>(Error.Create("ErrorCode", "Error"));

        int value = result.UnwrapOr(99);

        value.Should().Be(99);
    }

    [Fact]
    public async Task UnwrapOrAsync_ShouldRetrieveValueOnSuccess()
    {
        Task<Result<int>> resultTask = Task.FromResult(Result.Success(42));

        int value = await resultTask.UnwrapOrAsync(99);

        value.Should().Be(42);
    }

    [Fact]
    public async Task UnwrapOrAsync_ShouldReturnDefaultOnFailure()
    {
        Task<Result<int>> resultTask = Task.FromResult(Result.Failure<int>(Error.Create("ErrorCode", "Error")));

        int value = await resultTask.UnwrapOrAsync(99);

        value.Should().Be(99);
    }

    #region Do Tests

    [Fact]
    public void Do_VoidResult_ShouldExecuteActionOnSuccess()
    {
        // Arrange
        bool actionExecuted = false;
        Result result = Result.Success();

        // Act
        Result returnedResult = result.Do(() => actionExecuted = true);

        // Assert
        actionExecuted.Should().BeTrue();
        returnedResult.Should().BeSameAs(result);
        returnedResult.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Do_VoidResult_ShouldExecuteActionOnFailure()
    {
        // Arrange
        bool actionExecuted = false;
        Error error = Error.Create("ErrorCode", "Error message");
        Result result = Result.Failure(error);

        // Act
        Result returnedResult = result.Do(() => actionExecuted = true);

        // Assert
        actionExecuted.Should().BeTrue();
        returnedResult.Should().BeSameAs(result);
        returnedResult.IsFailure.Should().BeTrue();
        returnedResult.Error.Should().Be(error);
    }

    [Fact]
    public void Do_GenericResult_ShouldExecuteActionOnSuccess()
    {
        // Arrange
        bool actionExecuted = false;
        Result<string> result = Result.Success("test value");

        // Act
        Result<string> returnedResult = result.Do(() => actionExecuted = true);

        // Assert
        actionExecuted.Should().BeTrue();
        returnedResult.Should().BeSameAs(result);
        returnedResult.IsSuccess.Should().BeTrue();
        returnedResult.Value.Should().Be("test value");
    }

    [Fact]
    public void Do_GenericResult_ShouldExecuteActionOnFailure()
    {
        // Arrange
        bool actionExecuted = false;
        Error error = Error.Create("ErrorCode", "Error message");
        Result<string> result = Result.Failure<string>(error);

        // Act
        Result<string> returnedResult = result.Do(() => actionExecuted = true);

        // Assert
        actionExecuted.Should().BeTrue();
        returnedResult.Should().BeSameAs(result);
        returnedResult.IsFailure.Should().BeTrue();
        returnedResult.Error.Should().Be(error);
    }

    [Fact]
    public void Do_ShouldAllowChaining()
    {
        // Arrange
        int executionCount = 0;
        Result<int> result = Result.Success(42);

        // Act
        Result<int> chainedResult = result
            .Do(() => executionCount++)
            .Do(() => executionCount++)
            .Do(() => executionCount++);

        // Assert
        executionCount.Should().Be(3);
        chainedResult.IsSuccess.Should().BeTrue();
        chainedResult.Value.Should().Be(42);
    }

    [Fact]
    public void Do_ShouldExecuteActionEvenWhenExceptionIsThrown()
    {
        // Arrange
        bool actionExecuted = false;
        Result result = Result.Success();

        // Act & Assert
        result.Invoking(r => r.Do(() =>
        {
            actionExecuted = true;
            throw new InvalidOperationException("Test exception");
        })).Should().Throw<InvalidOperationException>().WithMessage("Test exception");

        actionExecuted.Should().BeTrue();
    }

    [Fact]
    public async Task DoAsync_VoidResult_ShouldExecuteActionOnSuccess()
    {
        // Arrange
        bool actionExecuted = false;
        Task<Result> resultTask = Task.FromResult(Result.Success());

        // Act
        Result returnedResult = await resultTask.DoAsync(async () =>
        {
            await Task.Delay(10);
            actionExecuted = true;
        });

        // Assert
        actionExecuted.Should().BeTrue();
        returnedResult.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task DoAsync_VoidResult_ShouldExecuteActionOnFailure()
    {
        // Arrange
        bool actionExecuted = false;
        Error error = Error.Create("ErrorCode", "Error message");
        Task<Result> resultTask = Task.FromResult(Result.Failure(error));

        // Act
        Result returnedResult = await resultTask.DoAsync(async () =>
        {
            await Task.Delay(10);
            actionExecuted = true;
        });

        // Assert
        actionExecuted.Should().BeTrue();
        returnedResult.IsFailure.Should().BeTrue();
        returnedResult.Error.Should().Be(error);
    }

    [Fact]
    public async Task DoAsync_GenericResult_ShouldExecuteActionOnSuccess()
    {
        // Arrange
        bool actionExecuted = false;
        Task<Result<string>> resultTask = Task.FromResult(Result.Success("test value"));

        // Act
        Result<string> returnedResult = await resultTask.DoAsync(async () =>
        {
            await Task.Delay(10);
            actionExecuted = true;
        });

        // Assert
        actionExecuted.Should().BeTrue();
        returnedResult.IsSuccess.Should().BeTrue();
        returnedResult.Value.Should().Be("test value");
    }

    [Fact]
    public async Task DoAsync_GenericResult_ShouldExecuteActionOnFailure()
    {
        // Arrange
        bool actionExecuted = false;
        Error error = Error.Create("ErrorCode", "Error message");
        Task<Result<string>> resultTask = Task.FromResult(Result.Failure<string>(error));

        // Act
        Result<string> returnedResult = await resultTask.DoAsync(async () =>
        {
            await Task.Delay(10);
            actionExecuted = true;
        });

        // Assert
        actionExecuted.Should().BeTrue();
        returnedResult.IsFailure.Should().BeTrue();
        returnedResult.Error.Should().Be(error);
    }

    [Fact]
    public async Task DoAsync_ShouldAllowChaining()
    {
        // Arrange
        int executionCount = 0;
        Task<Result<int>> resultTask = Task.FromResult(Result.Success(42));

        // Act
        Result<int> chainedResult = await resultTask
            .DoAsync(async () =>
            {
                await Task.Delay(5);
                executionCount++;
            });

        Result<int> finalResult = await Task.FromResult(chainedResult)
            .DoAsync(async () =>
            {
                await Task.Delay(5);
                executionCount++;
            });

        // Assert
        executionCount.Should().Be(2);
        finalResult.IsSuccess.Should().BeTrue();
        finalResult.Value.Should().Be(42);
    }

    [Fact]
    public async Task DoAsync_ShouldPropagateExceptions()
    {
        // Arrange
        Task<Result> resultTask = Task.FromResult(Result.Success());

        // Act & Assert
        Func<Task> act = async () => await resultTask.DoAsync(async () =>
        {
            await Task.Delay(10);
            throw new InvalidOperationException("Test exception");
        });

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Test exception");
    }

    [Fact]
    public void Do_WithSideEffects_ShouldExecuteForLoggingScenario()
    {
        // Arrange
        string logMessage = string.Empty;
        Result<int> result = Result.Success(42);

        // Act
        Result<int> resultWithLogging = result.Do(() => logMessage = "Operation completed");

        // Assert
        logMessage.Should().Be("Operation completed");
        resultWithLogging.IsSuccess.Should().BeTrue();
        resultWithLogging.Value.Should().Be(42);
    }

    [Fact]
    public void Do_WithSideEffects_ShouldExecuteForCleanupScenario()
    {
        // Arrange
        bool cleanupExecuted = false;
        Error error = Error.Create("ErrorCode", "Operation failed");
        Result result = Result.Failure(error);

        // Act
        Result resultWithCleanup = result.Do(() => cleanupExecuted = true);

        // Assert
        cleanupExecuted.Should().BeTrue();
        resultWithCleanup.IsFailure.Should().BeTrue();
        resultWithCleanup.Error.Should().Be(error);
    }

    #endregion
}
