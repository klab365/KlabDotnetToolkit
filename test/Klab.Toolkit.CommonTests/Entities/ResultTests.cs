using FluentAssertions;

namespace Klab.Toolkit.Common.Entities.Tests;

[TestClass]
public class ResultTests
{
    [TestMethod]
    public void SuccessTest()
    {
        Result<bool> result = Result<bool>.Success(true);
        _ = result.Value.Should().Be(true);
    }

    [TestMethod]
    public void FailureTest()
    {
        Result<bool> result = Result<bool>.Failure(new string[] { "error 1", "error 2" });

        _ = result.Errors.Should().HaveCount(2);
        _ = result.Succeeded.Should().Be(false);
        _ = result.Value.Should().BeNull();
    }
}
