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
    public async Task TapAsync_ShouldExecuteActionOnSuccess()
    {
        bool actionExecuted = false;
        Result.Success("Success").OnSuccess(async _ => await Task.Run(() => actionExecuted = true));
        await Task.Delay(100);

        actionExecuted.Should().BeTrue();
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
            .WithMessage("Cannot unwrap a failure result.");
    }
}
