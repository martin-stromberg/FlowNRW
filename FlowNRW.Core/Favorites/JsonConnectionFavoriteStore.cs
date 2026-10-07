using System.Text.Json;
using System.Text.Json.Serialization;

namespace FlowNRW.Core.Favorites;

/// <summary>Bounded atomic JSON storage for explicitly saved journey endpoint pairs.</summary>
public sealed partial class JsonConnectionFavoriteStore : IConnectionFavoriteStore
{
    private const int MaximumBytes = 256 * 1024;
    private const int CurrentVersion = 1;
    private readonly string path;
    private readonly SemaphoreSlim gate = new(1, 1);

    /// <summary>Creates a store at an application-owned path.</summary>
    /// <param name="path">Absolute or application-relative storage path.</param>
    public JsonConnectionFavoriteStore(string path) => this.path = Path.GetFullPath(path);

    /// <inheritdoc />
    public async Task<IReadOnlyList<ConnectionFavorite>> LoadAsync(CancellationToken cancellationToken = default)
    {
        await gate.WaitAsync(cancellationToken);
        try { return await ReadAsync(cancellationToken); }
        finally { gate.Release(); }
    }

    /// <inheritdoc />
    public async Task SaveAsync(IReadOnlyList<ConnectionFavorite> favorites, CancellationToken cancellationToken = default)
    {
        var document = new ConnectionFavoriteDocument { Version = CurrentVersion, Favorites = Validate(favorites) };
        var bytes = JsonSerializer.SerializeToUtf8Bytes(document, ConnectionFavoriteJsonContext.Default.ConnectionFavoriteDocument);
        if (bytes.Length > MaximumBytes) throw new InvalidDataException("Connection favorite file exceeds its limit.");
        await gate.WaitAsync(cancellationToken);
        string? temporary = null;
        try
        {
            await ReadAsync(cancellationToken);
            var directory = Path.GetDirectoryName(path)!;
            Directory.CreateDirectory(directory);
            temporary = Path.Combine(directory, ".connection-favorites-" + Guid.NewGuid().ToString("N") + ".tmp");
            await using (var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, FileOptions.Asynchronous | FileOptions.WriteThrough))
            {
                await stream.WriteAsync(bytes, cancellationToken);
                await stream.FlushAsync(cancellationToken);
                stream.Flush(true);
            }
            cancellationToken.ThrowIfCancellationRequested();
            File.Move(temporary, path, true);
            temporary = null;
        }
        finally
        {
            if (temporary is not null) try { File.Delete(temporary); } catch (IOException) { } catch (UnauthorizedAccessException) { }
            gate.Release();
        }
    }

    private async Task<ConnectionFavorite[]> ReadAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(path)) return [];
        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous);
        if (stream.Length > MaximumBytes) throw new InvalidDataException("Connection favorite file exceeds its limit.");
        using var buffer = new MemoryStream();
        await stream.CopyToAsync(buffer, cancellationToken);
        if (buffer.Length > MaximumBytes) throw new InvalidDataException("Connection favorite file exceeds its limit.");
        try
        {
            var document = JsonSerializer.Deserialize(buffer.ToArray(), ConnectionFavoriteJsonContext.Default.ConnectionFavoriteDocument)
                ?? throw new InvalidDataException("Invalid connection favorite file.");
            if (document.Version != CurrentVersion) throw new InvalidDataException("Unsupported connection favorite file version.");
            return Validate(document.Favorites);
        }
        catch (JsonException error) { throw new InvalidDataException("Invalid connection favorite file.", error); }
        catch (ArgumentException error) { throw new InvalidDataException("Invalid connection favorite file.", error); }
    }

    private static ConnectionFavorite[] Validate(IReadOnlyList<ConnectionFavorite>? favorites)
    {
        if (favorites is null || favorites.Count > 100) throw new InvalidDataException("Invalid connection favorites.");
        var seen = new HashSet<(string, string, string, string)>();
        var result = new List<ConnectionFavorite>();
        foreach (var favorite in favorites)
        {
            if (favorite is null || !Valid(favorite.Origin) || !Valid(favorite.Destination)) throw new InvalidDataException("Invalid connection favorite identity.");
            var key = (favorite.Origin.Source, favorite.Origin.Id, favorite.Destination.Source, favorite.Destination.Id);
            if (seen.Add(key)) result.Add(favorite);
        }
        return result.ToArray();
    }

    private static bool Valid(FlowNRW.Core.Transit.Stop stop) => stop is not null && !string.IsNullOrWhiteSpace(stop.Id) && !string.IsNullOrWhiteSpace(stop.Source)
        && !string.IsNullOrWhiteSpace(stop.Name) && stop.Id.Length <= 1024 && stop.Source.Length <= 256 && stop.Name.Length <= 1024;

    private sealed record ConnectionFavoriteDocument
    {
        public int Version { get; init; }
        public IReadOnlyList<ConnectionFavorite> Favorites { get; init; } = [];
    }

    [JsonSourceGenerationOptions(UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip)]
    [JsonSerializable(typeof(ConnectionFavoriteDocument))]
    private partial class ConnectionFavoriteJsonContext : JsonSerializerContext;
}
