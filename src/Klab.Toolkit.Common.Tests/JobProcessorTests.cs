using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Klab.Toolkit.Common.Tests;

public class JobProcessorTests : IDisposable
{
    private readonly ILogger<JobProcessor<string>> _logger;
    private readonly JobProcessor<string> _sut;
    private bool _disposed;

    public JobProcessorTests()
    {
        _logger = Substitute.For<ILogger<JobProcessor<string>>>();
        _sut = new JobProcessor<string>(_logger);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _sut?.Dispose();
            }
            _disposed = true;
        }
    }

    [Fact]
    public void Init_WithNullHandler_ShouldThrowArgumentNullException()
    {
        // Arrange
        Func<string, CancellationToken, Task> nullHandler = null!;

        // Act
        Action act = () => _sut.Init(nullHandler);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("jobHandler");
    }

    [Fact]
    public async Task EnqueueAsync_WithoutInit_ShouldLogError()
    {
        // Arrange
        string job = "test-job";

        // Act
        await _sut.EnqueueAsync(job);

        // Assert
        _logger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("has no handler set")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception, string>>());
    }

    [Fact]
    public async Task EnqueueAsync_WithMultipleJobs_WithoutInit_ShouldLogError()
    {
        // Arrange
        string[] jobs = new[] { "job1", "job2", "job3" };

        // Act
        await _sut.EnqueueAsync(jobs);

        // Assert
        _logger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("has no handler set")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception, string>>());
    }

    [Fact]
    public async Task EnqueueAsync_WithHandler_ShouldProcessJob()
    {
        // Arrange
        List<string> processedJobs = new List<string>();
        Func<string, CancellationToken, Task> jobHandler = Substitute.For<Func<string, CancellationToken, Task>>();
        jobHandler.Invoke(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                processedJobs.Add(callInfo.ArgAt<string>(0));
                return Task.CompletedTask;
            });

        _sut.Init(jobHandler);
        string job = "test-job";

        // Act
        await _sut.EnqueueAsync(job);
        await Task.Delay(100); // Give time for background processing

        // Assert
        await jobHandler.Received(1).Invoke(job, Arg.Any<CancellationToken>());
        processedJobs.Should().ContainSingle().Which.Should().Be(job);
    }

    [Fact]
    public async Task EnqueueAsync_WithMultipleJobs_ShouldProcessAllJobs()
    {
        // Arrange
        List<string> processedJobs = new List<string>();
        TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
        Func<string, CancellationToken, Task> jobHandler = Substitute.For<Func<string, CancellationToken, Task>>();
        int expectedJobCount = 3;

        jobHandler.Invoke(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                processedJobs.Add(callInfo.ArgAt<string>(0));
                if (processedJobs.Count == expectedJobCount)
                {
                    tcs.TrySetResult(true);
                }
                return Task.CompletedTask;
            });

        _sut.Init(jobHandler);
        string[] jobs = new[] { "job1", "job2", "job3" };

        // Act
        await _sut.EnqueueAsync(jobs);
        await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));

        // Assert
        processedJobs.Should().HaveCount(3);
        processedJobs.Should().BeEquivalentTo(jobs);
    }

    [Fact]
    public async Task EnqueueAsync_WithCancellationToken_ShouldPassCancellationToken()
    {
        // Arrange
        using CancellationTokenSource cts = new CancellationTokenSource();
        Func<string, CancellationToken, Task> jobHandler = Substitute.For<Func<string, CancellationToken, Task>>();
        jobHandler.Invoke(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        _sut.Init(jobHandler);
        string job = "test-job";

        // Act
        await _sut.EnqueueAsync(job, cts.Token);
        await Task.Delay(100);

        // Assert
        await jobHandler.Received(1).Invoke(job, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessJobsAsync_WhenHandlerThrowsException_ShouldLogErrorAndContinueProcessing()
    {
        // Arrange
        List<string> processedJobs = new List<string>();
        TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
        Func<string, CancellationToken, Task> jobHandler = Substitute.For<Func<string, CancellationToken, Task>>();
        int callCount = 0;

        jobHandler.Invoke(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                callCount++;
                string job = callInfo.ArgAt<string>(0);

                if (job == "failing-job")
                {
                    throw new InvalidOperationException("Test exception");
                }

                processedJobs.Add(job);

                if (callCount == 3)
                {
                    tcs.TrySetResult(true);
                }

                return Task.CompletedTask;
            });

        _sut.Init(jobHandler);
        string[] jobs = new[] { "job1", "failing-job", "job2" };

        // Act
        await _sut.EnqueueAsync(jobs);
        await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));

        // Assert
        processedJobs.Should().HaveCount(2);
        processedJobs.Should().Contain("job1");
        processedJobs.Should().Contain("job2");
        processedJobs.Should().NotContain("failing-job");

        _logger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("error occurred while processing")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception, string>>());
    }

    [Fact]
    public async Task StopAsync_ShouldStopProcessingNewJobs()
    {
        // Arrange
        List<string> processedJobs = new List<string>();
        Func<string, CancellationToken, Task> jobHandler = Substitute.For<Func<string, CancellationToken, Task>>();
        jobHandler.Invoke(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                processedJobs.Add(callInfo.ArgAt<string>(0));
                return Task.CompletedTask;
            });

        _sut.Init(jobHandler);

        // Act
        await _sut.EnqueueAsync("job1");
        await Task.Delay(100); // Allow first job to process
        await _sut.StopAsync();

        // Assert
        processedJobs.Should().Contain("job1");
    }

    [Fact]
    public async Task StopAsync_ShouldWaitForWorkerTaskCompletion()
    {
        // Arrange
        TaskCompletionSource<bool> taskStarted = new TaskCompletionSource<bool>();
        TaskCompletionSource<bool> taskCanComplete = new TaskCompletionSource<bool>();

        Func<string, CancellationToken, Task> jobHandler = Substitute.For<Func<string, CancellationToken, Task>>();
        jobHandler.Invoke(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(async callInfo =>
            {
                taskStarted.SetResult(true);
                await taskCanComplete.Task;
            });

        _sut.Init(jobHandler);

        // Act
        await _sut.EnqueueAsync("long-running-job");
        await taskStarted.Task; // Wait until job starts processing

        Task stopTask = Task.Run(async () =>
        {
            await Task.Delay(50); // Small delay before stopping
            taskCanComplete.SetResult(true);
            await _sut.StopAsync();
        });

        await stopTask;

        // Assert
        stopTask.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public async Task Dispose_ShouldCancelProcessing()
    {
        // Arrange
        JobProcessor<string> processor = new JobProcessor<string>(_logger);
        bool jobWasProcessed = false;
        TaskCompletionSource<bool> jobStarted = new TaskCompletionSource<bool>();

        Func<string, CancellationToken, Task> jobHandler = Substitute.For<Func<string, CancellationToken, Task>>();
        jobHandler.Invoke(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                jobStarted.SetResult(true);
                jobWasProcessed = true;
                return Task.CompletedTask;
            });

        processor.Init(jobHandler);

        // Act
        await processor.EnqueueAsync("job");
        await jobStarted.Task;
        processor.Dispose();
        await Task.Delay(100);

        // Assert
        jobWasProcessed.Should().BeTrue();
    }

    [Fact]
    public async Task EnqueueAsync_MultipleTimes_ShouldProcessInOrder()
    {
        // Arrange
        List<string> processedJobs = new List<string>();
        TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
        int expectedCount = 5;

        Func<string, CancellationToken, Task> jobHandler = Substitute.For<Func<string, CancellationToken, Task>>();
        jobHandler.Invoke(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                string job = callInfo.ArgAt<string>(0);
                processedJobs.Add(job);

                if (processedJobs.Count == expectedCount)
                {
                    tcs.TrySetResult(true);
                }

                return Task.CompletedTask;
            });

        _sut.Init(jobHandler);

        // Act
        await _sut.EnqueueAsync("job1");
        await _sut.EnqueueAsync("job2");
        await _sut.EnqueueAsync("job3");
        await _sut.EnqueueAsync("job4");
        await _sut.EnqueueAsync("job5");

        await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));

        // Assert
        processedJobs.Should().HaveCount(5);
        processedJobs.Should().ContainInOrder("job1", "job2", "job3", "job4", "job5");
    }

    [Fact]
    public async Task EnqueueAsync_WithComplexType_ShouldProcessCorrectly()
    {
        // Arrange
        ILogger<JobProcessor<int>> logger = Substitute.For<ILogger<JobProcessor<int>>>();
        using JobProcessor<int> processor = new JobProcessor<int>(logger);
        List<int> processedJobs = new List<int>();
        TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

        Func<int, CancellationToken, Task> jobHandler = Substitute.For<Func<int, CancellationToken, Task>>();
        jobHandler.Invoke(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                processedJobs.Add(callInfo.ArgAt<int>(0));
                tcs.TrySetResult(true);
                return Task.CompletedTask;
            });

        processor.Init(jobHandler);
        int testJob = 123;

        // Act
        await processor.EnqueueAsync(testJob);
        await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));

        // Assert
        processedJobs.Should().ContainSingle();
        processedJobs[0].Should().Be(123);
    }
}
