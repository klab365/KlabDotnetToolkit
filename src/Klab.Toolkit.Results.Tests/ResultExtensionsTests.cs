using System;
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
    public void Tap_ShouldExecuteActionOnSuccess()
    {
        bool actionExecuted = false;
        Result.Success("Success").Tap(_ => actionExecuted = true);

        actionExecuted.Should().BeTrue();
    }

    [Fact]
    public void Tap_ShouldExecuteActionOnSuccessWithGenericValue()
    {
        string capturedValue = string.Empty;
        Result.Success("Success").Tap(value => capturedValue = value);

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
    public void Ensure_ShouldReturnSuccessIfConditionIsMet()
    {
        Result<int> result = Result.Success(42).Ensure(x => x > 0, Error.Create("ErrorCode", "Error"));

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Ensure_ShouldReturnFailureIfConditionIsNotMet()
    {
        Result<int> result = Result.Success(0).Ensure(x => x > 0, Error.Create("ErrorCode", "Error"));

        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Be("Error");
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
