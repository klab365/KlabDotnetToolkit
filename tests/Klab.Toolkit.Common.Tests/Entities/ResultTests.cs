using FluentAssertions;

namespace Klab.Toolkit.Common.Entities.Tests;

[TestClass]
public class ResultTests
{
    [TestMethod]
    public void SuccessTest()
    {
        Result<int> result = 100;
        result.IsSuccess.Should().Be(true);
        result.Value.Should().Be(100);
    }

    [TestMethod]
    public void SuccessTest_With_Class()
    {
        Result<TestClass> result = new TestClass();
        result.IsSuccess.Should().Be(true);
        result.Value?.Should().NotBeNull();
    }

    [TestMethod]
    public void FailureTest()
    {
        Result result = Result.Failure(new Error("000", "test error", ""));

        result.Error.Message.Should().Be("test error");
        result.IsSuccess.Should().Be(false);
    }

    [TestMethod]
    public void FailureTest_WithClass()
    {
        Result result = Result.Failure("error");

        result.Error.Message.Should().Be("error");
        result.IsSuccess.Should().Be(false);
    }
}

internal sealed class TestClass { }
