using FlowNRW.Core.Transit;

namespace FlowNRW.Tests.Transit;

internal sealed class AdapterGateway : ITransitHttpGateway
{
    internal List<Uri> Requests { get; } = [];
    internal Queue<ProviderResult<string>> Responses { get; } = new();
    public Task<ProviderResult<string>> GetAsync(string provider, Uri uri, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested(); Requests.Add(uri);
        return Task.FromResult(Responses.Dequeue() with { Source = provider });
    }
    internal void Add(string json) => Responses.Enqueue(new() { Items = [json] });
    internal static string Fixture(string name)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !Directory.Exists(Path.Combine(directory.FullName, "docs", "help", "fahrplanauskunft", "fixtures"))) directory = directory.Parent;
        return File.ReadAllText(Path.Combine(directory!.FullName, "docs", "help", "fahrplanauskunft", "fixtures", name));
    }
}
