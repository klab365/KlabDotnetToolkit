using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Klab.Toolkit.Common;
using Klab.Toolkit.Common.Extensions;
using Klab.Toolkit.DI.DependencyFactory;
using Klab.Toolkit.Messaging;
using Klab.Toolkit.Results;
using Klab.Toolkit.ValueObjects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

HostApplicationBuilder builder = Host.CreateApplicationBuilder();
IServiceCollection services = builder.Services;
Klab.Toolkit.Common.DependencyInjection.AddKlabToolkitCommon(services);
services.AddFactoryMethodTransient<ISmokeService, SmokeService>("smoke");
services.AddEventHandler<SmokeEvent, SmokeEventHandler>();
services.AddRequestResponseHandler<SmokeRequest, Result<string>, SmokeRequestHandler>();
services.AddStreamRequestResponseHandler<SmokeStreamRequest, string, SmokeStreamRequestHandler>();
services.AddMessagingModule<NullMessagingLogger>();
using IHost host = builder.Build();
await host.StartAsync();
IServiceProvider provider = host.Services;

IDependencyFactory<ISmokeService> factory = provider.GetRequiredService<IDependencyFactory<ISmokeService>>();
if (factory.GetInstance("smoke").GetValue() != "factory")
{
    return 3;
}

if (!"smoke@example.com".IsEmail() || Email.Create("smoke@example.com").Value != "smoke@example.com" || IpAddress.Create("127.0.0.1").Value != "127.0.0.1" || ComPort.Create("COM1").Value != "COM1")
{
    return 4;
}

IMediator mediator = provider.GetRequiredService<IMediator>();
await mediator.PublishAsync(new SmokeEvent(), CancellationToken.None);
await SmokeEventHandler.Processed.Task.WaitAsync(TimeSpan.FromSeconds(5));
Result<string> requestResult = await mediator.SendAsync(new SmokeRequest(), CancellationToken.None);
if (!requestResult.IsSuccess || requestResult.Value != "response")
{
    return 1;
}

await foreach (string item in mediator.Stream(new SmokeStreamRequest(), CancellationToken.None))
{
    if (item != "stream")
    {
        return 2;
    }
}

_ = provider.GetRequiredService<ITimeProvider>();
_ = "aot".ToUpperInvariant();
_ = Result.Success();
_ = typeof(DependencyFactory<>).Name;

string recordingPath = Path.Combine(Path.GetTempPath(), $"klab-aot-recording-{Guid.NewGuid():N}.json");
FileMessagingLogger fileLogger = new(new MessagingModuleConfiguration { MessagingLoggerPath = recordingPath });
await fileLogger.StartAsync(CancellationToken.None);
await fileLogger.LogEventAsync(new SmokeEvent(), [Result.Success()]);
await fileLogger.FlushAsync();
await fileLogger.StopAsync(CancellationToken.None);
string recording = await File.ReadAllTextAsync(recordingPath);
File.Delete(recordingPath);
if (!recording.Contains("SmokeEvent", StringComparison.Ordinal) || !recording.Contains("recorded", StringComparison.Ordinal))
{
    return 5;
}

await host.StopAsync();
return 0;

internal interface ISmokeService
{
    string GetValue();
}

internal sealed class SmokeService : ISmokeService
{
    public string GetValue() => "factory";
}

internal sealed record SmokeEvent : EventBase, IRecordable
{
    public string ToRecordingDataJson() => "{\"Data\":\"recorded\"}";
}
internal sealed record SmokeRequest : IRequest<Result<string>>, IRecordable
{
    public string ToRecordingDataJson() => "{}";
}
internal sealed record SmokeStreamRequest : IStreamRequest<string>, IRecordable
{
    public string ToRecordingDataJson() => "{}";
}

internal sealed class SmokeEventHandler : IEventHandler<SmokeEvent>
{
    public static TaskCompletionSource<bool> Processed { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public Task<Result> Handle(SmokeEvent @event, CancellationToken cancellationToken)
    {
        Console.WriteLine("AOT smoke event processed");
        Processed.TrySetResult(true);
        return Task.FromResult(Result.Success());
    }
}

internal sealed class SmokeRequestHandler : IRequestHandler<SmokeRequest, Result<string>>
{
    public Task<Result<string>> HandleAsync(SmokeRequest request, CancellationToken cancellationToken)
    {
        return Task.FromResult(Result.Success("response"));
    }
}

internal sealed class SmokeStreamRequestHandler : IStreamRequestHandler<SmokeStreamRequest, string>
{
    public IAsyncEnumerable<string> HandleAsync(SmokeStreamRequest request, CancellationToken cancellationToken)
    {
        return GetItems();
    }

    private static async IAsyncEnumerable<string> GetItems()
    {
        yield return "stream";
        await Task.CompletedTask;
    }
}
