using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Klab.Toolkit.Results;
using Microsoft.Extensions.Hosting;

namespace Klab.Toolkit.Messaging;

/// <summary>
/// File-based implementation of IMessagingLogger that uses a channel-backed background worker
/// so that callers are never blocked by I/O.
/// </summary>
public sealed class FileMessagingLogger : BackgroundService, IMessagingLogger
{
    private readonly string _logFilePath;
    private readonly Channel<object> _channel = Channel.CreateUnbounded<object>(new UnboundedChannelOptions { SingleReader = true });

    /// <summary>
    /// Initializes a new instance of the <see cref="FileMessagingLogger"/> class.
    /// </summary>
    public FileMessagingLogger(MessagingModuleConfiguration configuration)
    {
        _logFilePath = Environment.ExpandEnvironmentVariables(configuration.MessagingLoggerPath);
    }

    /// <inheritdoc/>
    public ValueTask LogEventAsync(EventBase @event, Result[] handlerResults)
    {
        string? eventData = @event is IRecordable recordable ? recordable.ToRecordingDataJson() : null;
        object entry = new RecordingEntry(DateTime.UtcNow, "Event", @event.GetType().Name, eventData, null, GenerateResultLogs(handlerResults));
        _channel.Writer.TryWrite(entry);
        return default;
    }

    /// <inheritdoc/>
    public ValueTask LogCommandAsync(Type requestType, object requestData, object? response)
    {
        string? request = requestData is IRecordable recordable ? recordable.ToRecordingDataJson() : null;
        string? responseData = ExtractResponseValue(response);
        object entry = new RecordingEntry(DateTime.UtcNow, "Command", requestType.Name, request, responseData);
        _channel.Writer.TryWrite(entry);
        return default;
    }

    /// <inheritdoc/>
    public ValueTask LogStreamRequestAsync(Type requestType, object requestData, object? response)
    {
        string? request = requestData is IRecordable recordable ? recordable.ToRecordingDataJson() : null;
        string? responseData = ExtractResponseValue(response);
        object entry = new RecordingEntry(DateTime.UtcNow, "StreamRequest", requestType.Name, request, responseData);
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
        await foreach (object entry in _channel.Reader.ReadAllAsync(stoppingToken))
        {
            if (entry is FlushMarker marker)
            {
                marker.Completion.TrySetResult(true);
                continue;
            }

            await AppendEntryToFileAsync(entry, stoppingToken);
        }
    }

    private async Task AppendEntryToFileAsync(object entry, CancellationToken cancellationToken)
    {
        RecordingEntry recording = (RecordingEntry)entry;
        using MemoryStream buffer = new();
        using (Utf8JsonWriter writer = new(buffer))
        {
            writer.WriteStartObject();
            writer.WriteString("Timestamp", recording.Timestamp);
            writer.WriteString("Type", recording.Type);
            writer.WriteString("RequestType", recording.MessageType);
            if (recording.Type == "Event")
            {
                WriteJsonData(writer, "Event", recording.Data);
            }
            else
            {
                WriteJsonData(writer, "Request", recording.Data);
                WriteJsonData(writer, "Response", recording.Response);
            }
            if (recording.Results is not null)
            {
                writer.WritePropertyName("Results");
                writer.WriteStartArray();
                foreach (object result in recording.Results)
                {
                    writer.WriteStringValue(((ResultLog)result).ErrorMessage);
                }
                writer.WriteEndArray();
            }
            writer.WriteEndObject();
        }

        await File.AppendAllTextAsync(_logFilePath, Encoding.UTF8.GetString(buffer.ToArray()) + Environment.NewLine, cancellationToken);
    }

    private static string? ExtractResponseValue(object? response)
    {
        return response switch
        {
            IResultWithValue { IsSuccess: true } result when result.GetValue() is IRecordable recordable => recordable.ToRecordingDataJson(),
            IRecordable recordable => recordable.ToRecordingDataJson(),
            _ => null
        };
    }

    private static void WriteJsonData(Utf8JsonWriter writer, string propertyName, string? json)
    {
        if (json is null)
        {
            return;
        }

        using JsonDocument document = JsonDocument.Parse(json);
        writer.WritePropertyName(propertyName);
        document.RootElement.WriteTo(writer);
    }

    private static IEnumerable<object> GenerateResultLogs(Result[] results)
    {
        if (results.All(r => r.IsSuccess))
        {
            return [];
        }

        return results
            .Where(r => !r.IsSuccess)
            .Select(r => new ResultLog(GetErrorMessageSafely(r)));
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

    private sealed record RecordingEntry(DateTime Timestamp, string Type, string MessageType, string? Data, string? Response, IEnumerable<object>? Results = null);

    private sealed record ResultLog(string? ErrorMessage);

    private sealed class FlushMarker
    {
        public TaskCompletionSource<bool> Completion { get; }

        public FlushMarker(TaskCompletionSource<bool> completion)
        {
            Completion = completion;
        }
    }
}
