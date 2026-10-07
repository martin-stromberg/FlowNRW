using System.Text.Json;
using System.Text.Json.Serialization;
using FlowNRW.Core.Transit;

namespace FlowNRW.Core.Favorites;

/// <summary>Bounded atomic JSON storage containing technical stops and no location history.</summary>
public sealed class JsonFavoriteStore : IFavoriteStore
{
    private const int MaximumBytes = 256 * 1024;
    private readonly string path;
    private readonly SemaphoreSlim gate = new(1, 1);

    /// <summary>Creates a store at an application-owned file path.</summary>
    /// <param name="path">Absolute path supplied by the platform.</param>
    public JsonFavoriteStore(string path) { this.path = Path.GetFullPath(path); }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Stop>> LoadAsync(CancellationToken cancellationToken = default)
    {
        await gate.WaitAsync(cancellationToken);
        try { return await ReadAsync(cancellationToken); }
        finally { gate.Release(); }
    }

    /// <inheritdoc />
    public async Task SaveAsync(IReadOnlyList<Stop> stops, CancellationToken cancellationToken = default)
    {
        var validated = Validate(stops);
        var bytes = JsonSerializer.SerializeToUtf8Bytes(validated, FavoriteJsonContext.Default.StopArray);
        if (bytes.Length > MaximumBytes) throw new InvalidDataException("Favorite file exceeds its limit.");
        await gate.WaitAsync(cancellationToken);
        string? temporary = null;
        try
        {
            await ReadAsync(cancellationToken);
            var directory = Path.GetDirectoryName(path)!;
            Directory.CreateDirectory(directory);
            temporary = Path.Combine(directory, ".favorites-" + Guid.NewGuid().ToString("N") + ".tmp");
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
            if (temporary is not null)
            {
                try { File.Delete(temporary); }
                catch (IOException) { }
                catch (UnauthorizedAccessException) { }
            }
            gate.Release();
        }
    }

    private async Task<Stop[]> ReadAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(path)) return [];
        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous);
        if (stream.Length > MaximumBytes) throw new InvalidDataException("Favorite file exceeds its limit.");
        using var buffer = new MemoryStream();
        var chunk = new byte[4096];
        int read;
        while ((read = await stream.ReadAsync(chunk, cancellationToken)) > 0)
        {
            if (buffer.Length + read > MaximumBytes) throw new InvalidDataException("Favorite file exceeds its limit.");
            buffer.Write(chunk, 0, read);
        }
        try
        {
            return Validate(JsonSerializer.Deserialize(buffer.ToArray(), FavoriteJsonContext.Default.StopArray)
                ?? throw new InvalidDataException("Invalid favorite file."));
        }
        catch (JsonException error) { throw new InvalidDataException("Invalid favorite file.", error); }
        catch (ArgumentException error) { throw new InvalidDataException("Invalid favorite file.", error); }
    }

    private static Stop[] Validate(IReadOnlyList<Stop> stops)
    {
        if (stops.Count > 100) throw new InvalidDataException("At most 100 favorites are supported.");
        var keys = new HashSet<(string Source, string Id)>();
        var result = new List<Stop>();
        foreach (var stop in stops)
        {
            if (stop is null || string.IsNullOrWhiteSpace(stop.Id) || string.IsNullOrWhiteSpace(stop.Source)
                || stop.Id.Length > 1024 || stop.Source.Length > 256 || stop.Name is null || stop.Name.Length > 1024 || stop.Dhid?.Length > 1024)
                throw new InvalidDataException("Invalid favorite identity.");
            if (keys.Add((stop.Source, stop.Id))) result.Add(stop);
        }
        return result.ToArray();
    }
}

[JsonSourceGenerationOptions(UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow)]
[JsonSerializable(typeof(Stop[]))]
internal partial class FavoriteJsonContext : JsonSerializerContext;
