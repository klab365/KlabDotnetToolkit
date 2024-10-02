using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Klab.Toolkit.Event.Tests;

public class StreamRequestResponseTests
{
    private readonly IEventBus _eventBus;

    public StreamRequestResponseTests()
    {
        IHost host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddStreamRequestResponseHandler<SingRequest, string, SingRequestHandler>();
                services.UseEventModule();
            })
            .Build();

        _eventBus = host.Services.GetRequiredService<IEventBus>();
        host.Start();
    }

    [Fact]
    public async Task Stream_ShouldReturnCorrectOrderResponses()
    {
        SingRequest req = new();

        // act
        IAsyncEnumerable<string> res = _eventBus.Stream(req, CancellationToken.None);

        // assert
        List<string> result = new();
        await foreach (string item in res)
        {
            result.Add(item);
        }
        result.Should().HaveCount(3);
        result.Should().ContainInOrder("Sing1", "Sing2", "Sing3");
    }
}

internal sealed class SingRequestHandler : IStreamRequestHandler<SingRequest, string>
{
    public async IAsyncEnumerable<string> HandleAsync(SingRequest request, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        yield return await Task.Run(() => "Sing1");
        yield return await Task.Run(() => "Sing2");
        yield return await Task.Run(() => "Sing3");
    }
}

internal sealed record SingRequest : IStreamRequest<string>;
