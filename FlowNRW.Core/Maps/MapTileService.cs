using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace FlowNRW.Core.Maps;

/// <summary>Safe raster response with explicit stale/error state.</summary>
/// <param name="Data">PNG data URI or empty on failure.</param>
/// <param name="Stale">Whether an expired cached tile was used.</param>
/// <param name="Error">Whether fresh map data could not be obtained.</param>
/// <returns>Immutable tile result.</returns>
public sealed record MapTile(string Data, bool Stale = false, bool Error = false);

/// <summary>Bounded asynchronous tile source.</summary>
public interface IMapTileService
{
    /// <summary>Fetches a validated raster tile.</summary>
    /// <param name="zoom">Zoom level.</param>
    /// <param name="x">Column.</param>
    /// <param name="y">Row.</param>
    /// <param name="cancellationToken">Page lifetime.</param>
    /// <returns>Image and explicit failure state.</returns>
    Task<MapTile> GetAsync(int zoom, int x, int y, CancellationToken cancellationToken);
}

/// <summary>HTTPS tile transport with conditional requests and a bounded technical disk cache.</summary>
public sealed class MapTileService : IMapTileService, IDisposable
{
    private readonly HttpClient client;
    private readonly MapOptions options;
    private readonly string directory;
    private readonly SemaphoreSlim gate = new(1, 1);

    /// <summary>Creates the shared tile gateway. The client must reject redirects.</summary>
    /// <param name="client">Dedicated HTTP client.</param>
    /// <param name="options">Validated provider settings.</param>
    /// <param name="directory">Dedicated technical cache directory.</param>
    public MapTileService(HttpClient client, MapOptions options, string directory)
    {
        options.Validate(); this.client = client; this.options = options; this.directory = directory;
    }

    /// <inheritdoc />
    public async Task<MapTile> GetAsync(int zoom, int x, int y, CancellationToken cancellationToken)
    {
        var uri = options.TileUri(zoom, x, y);
        await gate.WaitAsync(cancellationToken);
        try
        {
            var key = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(uri.AbsoluteUri)));
            var path = Path.Combine(directory, key + ".json");
            var cached = await ReadAsync(path, cancellationToken);
            if (cached is not null && cached.Expires > DateTimeOffset.UtcNow) return new(cached.Data);
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(options.Timeout);
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, uri);
                request.Headers.UserAgent.ParseAdd("FlowNRW/0.0.1 (+https://github.com/martin-stromberg/FlowNRW)");
                request.Headers.Accept.ParseAdd("image/png");
                if (cached?.ETag is { } tag) request.Headers.TryAddWithoutValidation("If-None-Match", tag);
                if (cached?.Modified is { } modified) request.Headers.IfModifiedSince = modified;
                using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, timeout.Token);
                var expires = Expiry(response);
                if (response.StatusCode == HttpStatusCode.NotModified && cached is not null)
                {
                    if (response.Headers.CacheControl?.NoStore == true) TryDelete(path);
                    else await SaveAsync(path, cached with
                    {
                        Expires = expires,
                        ETag = response.Headers.ETag?.ToString() ?? cached.ETag,
                        AllowStale = response.Headers.CacheControl is { } control ? !control.MustRevalidate && !control.NoCache : cached.AllowStale
                    }, timeout.Token);
                    return new(cached.Data);
                }
                if (!response.IsSuccessStatusCode || response.Content.Headers.ContentLength > 1024 * 1024)
                    return Failure(cached);
                await using var stream = await response.Content.ReadAsStreamAsync(timeout.Token);
                using var bytes = new MemoryStream();
                var buffer = new byte[8192];
                int count;
                while ((count = await stream.ReadAsync(buffer, timeout.Token)) > 0)
                {
                    if (bytes.Length + count > 1024 * 1024) return Failure(cached);
                    bytes.Write(buffer, 0, count);
                }
                var data = bytes.ToArray();
                if (data.Length < 8 || !data.AsSpan(0, 8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }))
                    return Failure(cached);
                var entry = new TileEntry("data:image/png;base64," + Convert.ToBase64String(data), expires,
                    response.Headers.ETag?.ToString(), response.Content.Headers.LastModified,
                    response.Headers.CacheControl?.MustRevalidate != true && response.Headers.CacheControl?.NoCache != true);
                if (response.Headers.CacheControl?.NoStore == true) TryDelete(path);
                else await SaveAsync(path, entry, timeout.Token);
                return new(entry.Data);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested) { return Failure(cached); }
            catch (HttpRequestException) { return Failure(cached); }
            catch (IOException) { return Failure(cached); }
        }
        finally { gate.Release(); }
    }

    private static DateTimeOffset Expiry(HttpResponseMessage response)
    {
        var now = DateTimeOffset.UtcNow;
        if (response.Headers.CacheControl?.NoCache == true) return now;
        if (response.Headers.CacheControl?.MaxAge is { } max)
        {
            var age = response.Headers.Age ?? TimeSpan.Zero;
            var apparentAge = response.Headers.Date is { } date ? now - date : TimeSpan.Zero;
            if (apparentAge > age) age = apparentAge;
            return now + (max > age ? max - age : TimeSpan.Zero);
        }
        return response.Content.Headers.Expires ?? now.AddDays(7);
    }

    private static MapTile Failure(TileEntry? entry) => entry is not { AllowStale: true } ? new("", Error: true) : new(entry.Data, Stale: true, Error: true);

    private static async Task<TileEntry?> ReadAsync(string path, CancellationToken cancellationToken)
    {
        try
        {
            if (!File.Exists(path) || new FileInfo(path).Length > 2 * 1024 * 1024) return null;
            var entry = JsonSerializer.Deserialize(await File.ReadAllTextAsync(path, cancellationToken), MapJsonContext.Default.TileEntry);
            return entry?.Data.StartsWith("data:image/png;base64,", StringComparison.Ordinal) == true ? entry : null;
        }
        catch (IOException) { return null; }
        catch (UnauthorizedAccessException) { return null; }
        catch (JsonException) { return null; }
    }

    private async Task SaveAsync(string path, TileEntry entry, CancellationToken cancellationToken)
    {
        try
        {
            Directory.CreateDirectory(directory);
            var json = JsonSerializer.Serialize(entry, MapJsonContext.Default.TileEntry);
            var bytes = Encoding.UTF8.GetByteCount(json);
            var files = new DirectoryInfo(directory).GetFiles("*.json").OrderBy(file => file.LastWriteTimeUtc).ToArray();
            var total = files.Sum(file => file.Length);
            foreach (var file in files)
            {
                if (total + bytes <= options.CacheMaxBytes) break;
                total -= file.Length; file.Delete();
            }
            await File.WriteAllTextAsync(path + ".tmp", json, cancellationToken);
            File.Move(path + ".tmp", path, true);
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
        finally { TryDelete(path + ".tmp"); }
    }

    private static void TryDelete(string path)
    {
        try { File.Delete(path); }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }

    /// <inheritdoc />
    public void Dispose() => gate.Dispose();

    internal sealed record TileEntry(string Data, DateTimeOffset Expires, string? ETag, DateTimeOffset? Modified, bool AllowStale);
}

[System.Text.Json.Serialization.JsonSerializable(typeof(MapTileService.TileEntry))]
internal partial class MapJsonContext : System.Text.Json.Serialization.JsonSerializerContext;
