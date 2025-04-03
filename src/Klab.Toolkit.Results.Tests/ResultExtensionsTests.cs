using System;
using FluentAssertions;
using Xunit;

namespace Klab.Toolkit.Results.Tests;

public class ResultExtensionsTests
{
    [Fact]
    public void Map_ShouldTransformSuccessValue()
    {
        Result<int> successResult = Result.Success(42);
        Result<int> mappedResult = successResult.Map(x => x * 2);

        Assert.True(mappedResult.IsSuccess);
        Assert.Equal(84, mappedResult.Value);
    }

    [Fact]
    public void Map_ShouldReturnFailureOnSourceFailure()
    {
        Result<int> failureResult = Result.Failure<int>(Error.Create("ErrorCode", "Error"));
        Result<int> mappedResult = failureResult.Map(x => x * 2);

        Assert.True(mappedResult.IsFailure);
        Assert.Equal("Error", mappedResult.Error.Message);
    }

    [Fact]
    public void Bind_ShouldChainOperationsOnSuccess()
    {
        Result<int> successResult = Result.Success(42);
        Result<int> chainedResult = successResult.Bind(x => Result.Success(x * 2));

        Assert.True(chainedResult.IsSuccess);
        Assert.Equal(84, chainedResult.Value);
    }

    [Fact]
    public void Bind_ShouldReturnFailureOnSourceFailure()
    {
        Result<int> failureResult = Result.Failure<int>(Error.Create("ErrorCode", "Error"));
        Result<int> chainedResult = failureResult.Bind(x => Result.Success(x * 2));

        Assert.True(chainedResult.IsFailure);
        Assert.Equal("Error", chainedResult.Error.Message);
    }

    [Fact]
    public void Tap_ShouldExecuteActionOnSuccess()
    {
        Result<string> successResult = Result.Success("Success");
        bool actionExecuted = false;
        successResult.Tap(result => actionExecuted = true);

        Assert.True(actionExecuted);
    }

    [Fact]
    public void Tap_ShouldExecuteActionOnSuccessWithGenericValue()
    {
        Result<string> successResult = Result.Success("Success");
        string result = string.Empty;
        successResult.Tap(value => result = value.Value);

        result.Should().Be("Success");
    }

    [Fact]
    public void OnFailure_ShouldExecuteActionOnFailure()
    {
        Result<string> failureResult = Result.Failure<string>(Error.Create("ErrorCode", "Error"));
        bool actionExecuted = false;
        failureResult.OnFailure(error => actionExecuted = true);

        Assert.True(actionExecuted);
    }

    [Fact]
    public void Ensure_ShouldReturnSuccessIfConditionIsMet()
    {
        Result<int> successResult = Result.Success(42);
        Result<int> ensuredResult = successResult.Ensure(x => x > 0, Error.Create("ErrorCode", "Error"));

        Assert.True(ensuredResult.IsSuccess);
    }

    [Fact]
    public void Ensure_ShouldReturnFailureIfConditionIsNotMet()
    {
        Result<int> failureResult = Result.Success(0);
        Result<int> ensuredResult = failureResult.Ensure(x => x > 0, Error.Create("ErrorCode", "Error"));

        Assert.True(ensuredResult.IsFailure);
        Assert.Equal("Error", ensuredResult.Error.Message);
    }

    [Fact]
    public void ToResult_ShouldWrapValueInSuccessResult()
    {
        int value = 42;
        Result<int> result = value.ToResult();

        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void Unwrap_ShouldRetrieveValueOnSuccess()
    {
        Result<int> successResult = Result.Success(42);
        int value = successResult.Unwrap();

        Assert.Equal(42, value);
    }

    [Fact]
    public void Unwrap_ShouldThrowExceptionOnFailure()
    {
        Result<int> failureResult = Result.Failure<int>(Error.Create("ErrorCode", "Error"));
        Assert.Throws<InvalidOperationException>(() => failureResult.Unwrap());
    }
}
