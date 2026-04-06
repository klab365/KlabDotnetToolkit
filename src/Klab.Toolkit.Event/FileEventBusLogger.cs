using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Klab.Toolkit.Results;
using Microsoft.Extensions.Hosting;

namespace Klab.Toolkit.Event;

/// <summary>
/// File-based implementation of IEventBusLogger that uses a channel-backed background worker
/// so that callers are never blocked by I/O.
/// </summary>
public sealed class FileEventBusLogger : BackgroundService, IEventBusLogger
{
    private readonly string _logFilePath;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly Channel<object> _channel = Channel.CreateUnbounded<object>(new UnboundedChannelOptions { SingleReader = true });

    /// <summary>
    /// Initializes a new instance of the <see cref="FileEventBusLogger"/> class.
    /// </summary>
    public FileEventBusLogger(EventModuleConfiguration configuration)
    {
        _logFilePath = Environment.ExpandEnvironmentVariables(configuration.EventBusLoggerPath);
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new JsonStringEnumConverter() },
            PropertyNameCaseInsensitive = true,
        };
        _jsonOptions.Converters.Add(new EventInterfaceJsonConverter());
    }

    /// <inheritdoc/>
    public ValueTask LogEventAsync(EventBase @event, Result[] handlerResults)
    {
        object entry = new {
            Timestamp = DateTime.UtcNow,
            Type = "Event",
            Event = @event,
            Results = GenerateResultLogs(handlerResults)
        };
        _channel.Writer.TryWrite(entry);
        return default;
    }

    /// <inheritdoc/>
    public ValueTask LogCommandAsync(Type requestType, object requestData, object? response)
    {
        object entry = new {
            Timestamp = DateTime.UtcNow,
            Type = "Command",
            RequestType = requestType.Name,
            Request = requestData,
            Response = ExtractResponseValue(response)
        };
        _channel.Writer.TryWrite(entry);
        return default;
    }

    /// <inheritdoc/>
    public ValueTask LogStreamRequestAsync(Type requestType, object requestData, object? response)
    {
        object entry = new {
            Timestamp = DateTime.UtcNow,
            Type = "StreamRequest",
            RequestType = requestType.Name,
            Request = requestData,
            Response = ExtractResponseValue(response)
        };
        _channel.Writer.TryWrite(entry);
        return default;
    }

    /// <summary>
    /// Waits until all currently enqueued log entries have been written to disk.
    /// Intended for use in tests.
    /// </summary>
    public async Task FlushAsync(CancellationToken cancellationToken = default)
    {
        TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        _channel.Writer.TryWrite(new FlushMarker(tcs));
        await Task.WhenAny(tcs.Task, Task.Delay(Timeout.Infinite, cancellationToken));
        cancellationToken.ThrowIfCancellationRequested();
    }

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        List<object> buffer = new List<object>();

        await foreach (object entry in _channel.Reader.ReadAllAsync(stoppingToken))
        {
            if (entry is FlushMarker marker)
            {
                if (buffer.Count > 0)
                {
                    await WriteBufferToFileAsync(buffer, stoppingToken);
                }
                marker.Completion.TrySetResult(true);
                continue;
            }

            buffer.Add(entry);
            await WriteBufferToFileAsync(buffer, stoppingToken);
        }
    }

    private async Task WriteBufferToFileAsync(List<object> buffer, CancellationToken cancellationToken)
    {
        string json = JsonSerializer.Serialize(buffer, _jsonOptions);
        await File.WriteAllTextAsync(_logFilePath, json, cancellationToken);
    }

    private static object? ExtractResponseValue(object? response)
    {
        if (response == null)
        {
            return null;
        }

        Type responseType = response.GetType();

        if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            System.Reflection.PropertyInfo? isSuccessProp = responseType.GetProperty("IsSuccess");
            if (isSuccessProp == null)
            {
                return null;
            }

            bool isSuccess = (bool)(isSuccessProp.GetValue(response) ?? false);
            if (isSuccess)
            {
                System.Reflection.PropertyInfo? valueProp = responseType.GetProperty("Value");
                if (valueProp != null)
                {
                    return valueProp.GetValue(response);
                }
            }

            return null;
        }

        if (response is Result)
        {
            return null;
        }

        return response;
    }

    private static IEnumerable<object> GenerateResultLogs(Result[] results)
    {
        if (results.All(r => r.IsSuccess))
        {
            return Array.Empty<object>();
        }

        return results
            .Where(r => !r.IsSuccess)
            .Select(r => new { ErrorMessage = GetErrorMessageSafely(r) });
    }

    private static string? GetErrorMessageSafely(Result result)
    {
        if (!result.IsFailure)
        {
            return null;
        }

        try
        {
            return result.Error?.Message;
        }
        catch
        {
            return "Unknown error";
        }
    }

    private sealed class FlushMarker
    {
        public TaskCompletionSource<bool> Completion { get; }

        public FlushMarker(TaskCompletionSource<bool> completion)
        {
            Completion = completion;
        }
    }
}
