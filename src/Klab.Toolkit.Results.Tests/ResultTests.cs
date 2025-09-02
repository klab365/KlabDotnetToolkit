using System;
using FluentAssertions;
using Xunit;

namespace Klab.Toolkit.Results.Tests;

public class ResultTests
{
    [Fact]
    public void SuccessTest()
    {
        Result<int> result = Result.Success(100);
        result.IsSuccess.Should().Be(true);
        result.Value.Should().Be(100);
    }

    [Fact]
    public void SuccessTest_With_Class()
    {
        Result<TestClass> result = Result<TestClass>.Success(new TestClass());
        result.IsSuccess.Should().Be(true);
        result.Value.Should().NotBeNull();
    }

    [Fact]
    public void FailureTest()
    {
        Result result = Result.Failure(Error.Create("0", "test error", ""));

        result.Error.Message.Should().Be("test error");
        result.IsSuccess.Should().Be(false);
    }

    [Fact]
    public void FailureTest_WithClass()
    {
        Result result = Result.Failure(Error.Create("1", "error"));

        result.Error.Message.Should().Be("error");
        result.IsSuccess.Should().Be(false);
    }

    [Fact]
    public void FailureTest_Generic()
    {
        Result<string> res = Result.Failure<string>(Error.Create("1", "error"));

        res.IsFailure.Should().BeTrue();
        res.Error.Message.Should().Be("error");
    }

    [Fact]
    public void Result_Success_And_Failure_Properties_Should_Work()
    {
        // Arrange
        Result successResult = Result.Success();
        Result failureResult = Result.Failure(Error.Create("1", "error"));

        // Act & Assert
        successResult.IsSuccess.Should().BeTrue();
        failureResult.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Implicit_Conversion_From_Error_To_Result_Should_Create_Failure()
    {
        // Arrange
        Error error = Error.Create("1", "error");

        // Act
        Result result = Result.Failure(error);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Explicit_Conversion_From_Value_To_ResultT_Using_Success_Should_Work()
    {
        // Arrange
        int value = 42;

        // Act
        Result<int> result = Result.Success(value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(value);
    }

    [Fact]
    public void Implicit_Conversion_From_Error_To_ResultT_Should_Create_Failure()
    {
        // Arrange
        Error error = Error.Create("1", "error");

        // Act
        Result<int> result = Result.Failure<int>(error);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Invoking(r => r.Value).Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ResultT_Success_And_Failure_Properties_Should_Work()
    {
        // Arrange
        Result<int> successResult = Result.Success(100);
        Result<int> failureResult = Result.Failure<int>(Error.Create("1", "error"));

        // Act & Assert
        successResult.IsSuccess.Should().BeTrue();
        failureResult.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Unwrap_From_ResultT_Should_Work_When_Successful()
    {
        // Arrange
        Result<int> result = Result.Success(123);

        // Act
        int value = result.Unwrap();

        // Assert
        value.Should().Be(123);
    }

    [Fact]
    public void Unwrap_From_ResultT_Should_Throw_When_Failed()
    {
        // Arrange
        Result<int> result = Result.Failure<int>(Error.Create("1", "error"));

        // Act & Assert
        result.Invoking(r => r.Unwrap()).Should().Throw<InvalidOperationException>();
    }
}

internal sealed class TestClass
{
    public string TestProperty { get; set; } = "test";

    public TestClass()
    {
        // nothing needed
    }
}
