using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Klab.Toolkit.Results;

namespace Klab.Toolkit.Event;

/// <summary>
/// JSON implementation of ICqrsEventLogger that writes to a file
/// </summary>
public class JsonCqrsEventLogger : ICqrsEventLogger
{
    private readonly string _logFilePath;
    private readonly List<object> _logs = new();
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    /// <summary>
    /// Creates a new JsonCqrsEventLogger
    /// </summary>
    /// <param name="logFilePath">Path to the log file</param>
    public JsonCqrsEventLogger(string logFilePath)
    {
        _logFilePath = logFilePath;
        _jsonSerializerOptions = new()
        {
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new JsonStringEnumConverter() },
            PropertyNameCaseInsensitive = true,
        };
        _jsonSerializerOptions.Converters.Add(new EventInterfaceJsonConverter());
    }

    /// <inheritdoc />
    public void LogEvent(EventBase @event)
    {
        object eventLog = new { Event = @event, Stage = "Published" };
        _logs.Add(eventLog);
        Flush();
    }

    /// <inheritdoc />
    public void LogRequest<T>(IRequest<T> request)
    {
        object requestLog = new { Request = request, Stage = "Sent" };
        _logs.Add(requestLog);
        Flush();
    }

    /// <inheritdoc />
    public void LogProcessedEvent(EventBase @event, IEnumerable<Result> results)
    {
        IEnumerable<object> failedResults = results
            .Where(r => !r.IsSuccess)
            .Select(r => new { r.Error });

        object eventLog = new {
            Event = @event,
            Stage = "Processed",
            Results = (object)(failedResults.Any() ? failedResults.ToList() : "All succeeded")
        };
        _logs.Add(eventLog);
        Flush();
    }

    private void Flush()
    {
        string jsonLog = JsonSerializer.Serialize(_logs, _jsonSerializerOptions);
        File.WriteAllText(_logFilePath, jsonLog);
    }
}
