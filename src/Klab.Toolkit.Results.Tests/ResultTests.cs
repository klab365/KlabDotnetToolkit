using FluentAssertions;

namespace Klab.Toolkit.Results.Tests;

[TestClass]
public class ResultTests
{
    [TestMethod]
    public void SuccessTest()
    {
        Result<int> result = Result.Success(100);
        result.IsSuccess.Should().Be(true);
        result.Value.Should().Be(100);
    }

    [TestMethod]
    public void SuccessTest_With_Class()
    {
        Result<TestClass> result = Result<TestClass>.Success(new TestClass());
        result.IsSuccess.Should().Be(true);
        result.Value.Should().NotBeNull();
    }

    [TestMethod]
    public void FailureTest()
    {
        Result result = Result.Failure(new InformativeError("0", "test error", ""));

        result.Error.Message.Should().Be("test error");
        result.IsSuccess.Should().Be(false);
    }

    [TestMethod]
    public void FailureTest_WithClass()
    {
        Result result = Result.Failure(new InformativeError("1", "error"));

        result.Error.Message.Should().Be("error");
        result.IsSuccess.Should().Be(false);
    }

    [TestMethod]
    public void FailureTest_Generic()
    {
        Result<string> res = Result.Failure<string>(new InformativeError("1", "error"));

        res.IsFailure.Should().BeTrue();
        res.Error.Message.Should().Be("error");
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
