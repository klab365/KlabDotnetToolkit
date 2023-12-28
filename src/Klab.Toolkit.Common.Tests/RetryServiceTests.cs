using System;
using System.Threading.Tasks;
using FluentAssertions;
using Klab.Toolkit.Results;

namespace Klab.Toolkit.Common.Tests;

[TestClass]
public class RetryServiceTests
{
    private RetryService _sut;

    [TestInitialize]
    public void Setup()
    {
        _sut = new();
    }

    [TestMethod]
    public async Task TryCallAsync_Should_BeSuccess()
    {
        Result result = await _sut.TryCallAsync(async (token) =>
        {
            await Task.Delay(100, token);
        }, TimeSpan.FromSeconds(1));

        result.IsSuccess.Should().BeTrue();
    }

    [TestMethod]
    public async Task TryCallAsyncTest_With_Failure()
    {
        Result result = await _sut.TryCallAsync(async (token) =>
        {
            await Task.Delay(100, token);
        }, TimeSpan.Zero);

        result.IsSuccess.Should().BeFalse();
    }
}
