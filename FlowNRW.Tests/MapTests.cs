using System.Net;
using System.Net.Http.Headers;
using FlowNRW.Core.Maps;
using FlowNRW.Core.Presentation;
using FlowNRW.Core.Transit;

namespace FlowNRW.Tests;

/// <summary>Map identity, geometry and bounded provider transport regression tests.</summary>
public sealed class MapTests
{
    /// <summary>Marker and list indexes resolve the same original identity and reject old sessions.</summary>
    [Fact]
    public async Task PreservesCandidatesAndRejectsStaleSelection()
    {
        var search = new ControlledSearchService(); var departures = new ControlledDepartureService();
        var monitor = new StopMonitorViewModel(search, departures, new DepartureTestNavigation(), 200);
        var map = new MapViewModel(monitor);
        var first = new Address { Name = "Same", Stop = new() { Id = "1", Name = "Same", Coordinate = new(51, 7) } };
        var second = first with { Stop = first.Stop with { Id = "2", Coordinate = null } };
        monitor.Lookup.Text = "Same"; var lookup = monitor.Lookup.SearchAsync();
        search.Pending[0].SetResult(new() { Items = [first, second] }); await lookup;
        map.ShowStops(); var old = map.Session;
        Assert.Same(first, map.Stations[0].Candidate);
        Assert.Equal(new GeoCoordinate(51, 7), map.Stations[0].Position);
        Assert.Null(map.Stations[1].Position); Assert.Contains("Keine Kartenposition", map.Stations[1].Label);
        await map.SelectAsync(old, -1); await map.SelectAsync(old, 20); Assert.Empty(departures.Pending);
        var opening = map.SelectAsync(old, 1); Assert.Same(second.Stop, departures.Stops[0]);
        departures.Pending[0].SetResult(new()); await opening;
        map.ShowStops(); await map.SelectAsync(old, 0); Assert.Single(departures.Pending);
        monitor.Lookup.Text = "Changed"; await map.SelectAsync(map.Session, 0); Assert.Single(departures.Pending);
    }

    /// <summary>Missing legs and unsupported polar coordinates never create artificial connections.</summary>
    [Fact]
    public void PreservesGeometryGapsAndClearsPreviousJourney()
    {
        var map = new MapViewModel(new(new ControlledSearchService(), new ControlledDepartureService(), new DepartureTestNavigation(), 200));
        var points = new GeoCoordinate[] { new(51, 7), new(52, 8), new(90, 0), new(53, 9), new(54, 10) };
        map.ShowJourney(new()
        {
            Legs = [new() { Line = new() { Name = "RE1" }, Geometry = new() { Coordinates = points } },
            new() { Walking = new() { Geometry = new() { Coordinates = [new(54, 10), new(54.1, 10.1)] } } }, new()]
        }, "source");
        Assert.Equal(3, map.Segments.Count); Assert.All(map.Segments, item => Assert.Equal(2, item.Points.Count));
        Assert.Equal("Fußweg", map.Segments[2].Label); Assert.Contains("Teilweiser", map.Status); Assert.Equal("source", map.Metadata);
        Assert.Empty(map.Stations);
        map.ShowJourney(new() { Legs = [new()] }, "new"); Assert.Empty(map.Segments); Assert.Contains("Keine darstellbare", map.Status);
        map.ShowStops(); Assert.Empty(map.Segments); Assert.Contains("Keine Haltestellen", map.Status);
    }

    /// <summary>Only HTTPS bounded tile coordinates and safe templates are accepted.</summary>
    /// <param name="template">Rejected endpoint.</param>
    [Theory]
    [InlineData("http://tiles.example/{z}/{x}/{y}.png")]
    [InlineData("https://user:password@tiles.example/{z}/{x}/{y}.png")]
    [InlineData("https://tiles.example/{z}/{x}/{unknown}.png")]
    [InlineData("https://tiles.example/{z}/{x}/{y}.png#fragment")]
    public void RejectsUnsafeTemplates(string template) => Assert.Throws<ArgumentException>(() => new MapOptions { TileUrl = template }.Validate());

    /// <summary>Fresh caches survive gateway instances and prevent redundant downloads.</summary>
    [Fact]
    public async Task CachesAcrossSessionsWithIdentifyingHeaders()
    {
        using var fixture = new TileFixture();
        fixture.Handler.Response = request =>
        {
            Assert.Contains("FlowNRW/", request.Headers.UserAgent.ToString());
            Assert.Equal("https", request.RequestUri!.Scheme);
            return TileFixture.Png("public, max-age=3600");
        };
        using (var service = fixture.Service()) Assert.False((await service.GetAsync(2, 1, 1, default)).Error);
        using (var service = fixture.Service()) Assert.False((await service.GetAsync(2, 1, 1, default)).Error);
        Assert.Equal(1, fixture.Handler.Calls);
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => fixture.Service().GetAsync(2, 4, 0, default));
    }

    /// <summary>Expired responses use validators; no-store and no-cache directives are respected.</summary>
    [Fact]
    public async Task RevalidatesAndHonorsNoStore()
    {
        using var fixture = new TileFixture(); using var service = fixture.Service();
        fixture.Handler.Response = _ => { var response = TileFixture.Png("no-cache"); response.Headers.ETag = new("\"v1\""); return response; };
        await service.GetAsync(2, 1, 1, default);
        fixture.Handler.Response = request => { Assert.Equal("\"v1\"", request.Headers.IfNoneMatch.Single().Tag); return new(HttpStatusCode.NotModified) { Headers = { CacheControl = CacheControlHeaderValue.Parse("max-age=3600") } }; };
        Assert.False((await service.GetAsync(2, 1, 1, default)).Error);
        Assert.False((await service.GetAsync(2, 1, 1, default)).Error); Assert.Equal(2, fixture.Handler.Calls);
        fixture.Handler.Response = _ => TileFixture.Png("no-store");
        await service.GetAsync(2, 2, 1, default); await service.GetAsync(2, 2, 1, default); Assert.Equal(4, fixture.Handler.Calls);
    }

    /// <summary>Offline states preserve permitted stale tiles and never bypass mandatory revalidation.</summary>
    /// <param name="control">Provider cache directive.</param>
    /// <param name="expectStale">Whether stale reuse is permitted.</param>
    [Theory]
    [InlineData("max-age=0", true)]
    [InlineData("max-age=0, must-revalidate", false)]
    public async Task ReportsOfflineAndStale(string control, bool expectStale)
    {
        using var fixture = new TileFixture(); using var service = fixture.Service();
        fixture.Handler.Response = _ => TileFixture.Png(control); await service.GetAsync(2, 1, 1, default);
        fixture.Handler.Response = _ => throw new HttpRequestException("offline");
        var result = await service.GetAsync(2, 1, 1, default);
        Assert.True(result.Error); Assert.Equal(expectStale, result.Stale); Assert.Equal(expectStale, result.Data.Length > 0);
        Assert.True((await service.GetAsync(2, 2, 1, default)).Error);
    }

    /// <summary>Invalid or oversized images fail safely and cancelled pages issue no new requests.</summary>
    [Fact]
    public async Task BoundsContentAndCancellation()
    {
        using var fixture = new TileFixture(); using var service = fixture.Service();
        fixture.Handler.Response = _ => new(HttpStatusCode.OK) { Content = new StringContent("<script>not an image</script>") };
        Assert.True((await service.GetAsync(2, 1, 1, default)).Error);
        fixture.Handler.Response = _ => new(HttpStatusCode.OK) { Content = new ByteArrayContent(new byte[1024 * 1024 + 1]) };
        Assert.True((await service.GetAsync(2, 1, 1, default)).Error);
        using var cancelled = new CancellationTokenSource(); cancelled.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.GetAsync(2, 1, 1, cancelled.Token));
        Assert.Equal(2, fixture.Handler.Calls);
    }

    /// <summary>Oversized cache contents are evicted and a stalled provider reaches a bounded failure.</summary>
    [Fact]
    public async Task BoundsDiskCacheAndTimeout()
    {
        using var fixture = new TileFixture();
        var image = new byte[800000]; new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }.CopyTo(image, 0);
        fixture.Handler.Response = _ => new(HttpStatusCode.OK) { Content = new ByteArrayContent(image) };
        using (var service = new MapTileService(new HttpClient(fixture.Handler, false), new() { CacheMaxBytes = 2 * 1024 * 1024 }, fixture.DirectoryPath))
        {
            await service.GetAsync(2, 0, 1, default); await service.GetAsync(2, 1, 1, default); await service.GetAsync(2, 2, 1, default);
            Assert.True(new DirectoryInfo(fixture.DirectoryPath).GetFiles().Sum(file => file.Length) <= 2 * 1024 * 1024);
            Assert.DoesNotContain(Directory.GetFiles(fixture.DirectoryPath), file => file.EndsWith(".tmp", StringComparison.Ordinal));
        }
        using var slow = new SlowTileHandler();
        using var timed = new MapTileService(new HttpClient(slow), new() { Timeout = TimeSpan.FromMilliseconds(30) }, fixture.DirectoryPath);
        Assert.True((await timed.GetAsync(2, 3, 1, default)).Error);
    }
}

internal sealed class TileFixture : IDisposable
{
    internal readonly string DirectoryPath = Path.Combine(Path.GetTempPath(), "flownrw-map-test-" + Guid.NewGuid().ToString("N"));
    internal TileHandler Handler { get; } = new();
    internal MapTileService Service() => new(new HttpClient(Handler, false), new(), DirectoryPath);
    internal static HttpResponseMessage Png(string control)
    {
        return new(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent([137, 80, 78, 71, 13, 10, 26, 10, 0]),
            Headers = { CacheControl = CacheControlHeaderValue.Parse(control) }
        };
    }
    public void Dispose() { if (Directory.Exists(DirectoryPath)) Directory.Delete(DirectoryPath, true); Handler.Dispose(); }
}

internal sealed class TileHandler : HttpMessageHandler
{
    internal Func<HttpRequestMessage, HttpResponseMessage> Response { get; set; } = _ => TileFixture.Png("max-age=3600");
    internal int Calls { get; private set; }
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Calls++; cancellationToken.ThrowIfCancellationRequested(); return Task.FromResult(Response(request));
    }
}

internal sealed class SlowTileHandler : HttpMessageHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
        return new(HttpStatusCode.OK);
    }
}
