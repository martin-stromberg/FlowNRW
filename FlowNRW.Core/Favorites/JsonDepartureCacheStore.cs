using System.Text.Json;
using System.Text.Json.Serialization;
using FlowNRW.Core.Transit;

namespace FlowNRW.Core.Favorites;

/// <summary>Bounded atomic JSON storage for successful favorite departure boards without coordinates.</summary>
public sealed class JsonDepartureCacheStore : IDepartureCacheStore
{
    private const int MaximumBytes = 512 * 1024;
    private const int MaximumEntries = 100;
    private const int MaximumEventsPerEntry = 100;
    private readonly string path;
    private readonly SemaphoreSlim gate = new(1, 1);

    /// <summary>Creates a store at an application-owned file path.</summary>
    /// <param name="path">Absolute path supplied by the platform.</param>
    public JsonDepartureCacheStore(string path) { this.path = Path.GetFullPath(path); }

    /// <inheritdoc />
    public async Task<IReadOnlyList<DepartureCacheEntry>> LoadAsync(CancellationToken cancellationToken = default)
    {
        await gate.WaitAsync(cancellationToken);
        try { return await ReadAsync(cancellationToken); }
        finally { gate.Release(); }
    }

    /// <inheritdoc />
    public async Task SaveAsync(IReadOnlyList<DepartureCacheEntry> entries, CancellationToken cancellationToken = default)
    {
        var validated = Validate(entries);
        await gate.WaitAsync(cancellationToken);
        try
        {
            await ReadAsync(cancellationToken);
            await WriteAsync(validated, cancellationToken);
        }
        finally { gate.Release(); }
    }

    /// <inheritdoc />
    public async Task UpsertAsync(DepartureCacheEntry entry, CancellationToken cancellationToken = default)
    {
        var validated = AssertSingle(entry);
        await gate.WaitAsync(cancellationToken);
        try
        {
            var existing = await ReadAsync(cancellationToken);
            var replacement = existing.Where(current => current.Key != validated.Key).Append(validated).ToArray();
            await WriteAsync(Validate(replacement), cancellationToken);
        }
        finally { gate.Release(); }
    }

    /// <inheritdoc />
    public async Task RemoveAsync(DepartureCacheKey key, CancellationToken cancellationToken = default)
    {
        ValidateKey(key);
        await gate.WaitAsync(cancellationToken);
        try
        {
            var existing = await ReadAsync(cancellationToken);
            var remaining = existing.Where(entry => entry.Key != key).ToArray();
            if (remaining.Length != existing.Length) await WriteAsync(remaining, cancellationToken);
        }
        finally { gate.Release(); }
    }

    /// <inheritdoc />
    public async Task RemoveOrphansAsync(IReadOnlyCollection<DepartureCacheKey> keys, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(keys);
        var retained = new HashSet<DepartureCacheKey>();
        foreach (var key in keys)
        {
            ValidateKey(key);
            retained.Add(key);
        }
        await gate.WaitAsync(cancellationToken);
        try
        {
            var existing = await ReadAsync(cancellationToken);
            var remaining = existing.Where(entry => retained.Contains(entry.Key)).ToArray();
            if (remaining.Length != existing.Length) await WriteAsync(remaining, cancellationToken);
        }
        finally { gate.Release(); }
    }

    private async Task<DepartureCacheEntry[]> ReadAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(path)) return [];
        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous);
        if (stream.Length > MaximumBytes) throw new InvalidDataException("Departure cache file exceeds its limit.");
        using var buffer = new MemoryStream();
        var chunk = new byte[4096];
        int read;
        while ((read = await stream.ReadAsync(chunk, cancellationToken)) > 0)
        {
            if (buffer.Length + read > MaximumBytes) throw new InvalidDataException("Departure cache file exceeds its limit.");
            buffer.Write(chunk, 0, read);
        }
        try
        {
            return Validate(JsonSerializer.Deserialize(buffer.ToArray(), DepartureCacheJsonContext.Default.DepartureCacheEntryArray)
                ?? throw new InvalidDataException("Invalid departure cache file."));
        }
        catch (JsonException error) { throw new InvalidDataException("Invalid departure cache file.", error); }
        catch (ArgumentException error) { throw new InvalidDataException("Invalid departure cache file.", error); }
    }

    private async Task WriteAsync(DepartureCacheEntry[] entries, CancellationToken cancellationToken)
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(entries, DepartureCacheJsonContext.Default.DepartureCacheEntryArray);
        if (bytes.Length > MaximumBytes) throw new InvalidDataException("Departure cache file exceeds its limit.");
        var directory = Path.GetDirectoryName(path)!;
        Directory.CreateDirectory(directory);
        string? temporary = Path.Combine(directory, ".departure-cache-" + Guid.NewGuid().ToString("N") + ".tmp");
        try
        {
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
        }
    }

    private static DepartureCacheEntry[] Validate(IReadOnlyList<DepartureCacheEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        if (entries.Count > MaximumEntries) throw new InvalidDataException("At most 100 departure cache entries are supported.");
        var keys = new HashSet<DepartureCacheKey>();
        var result = new List<DepartureCacheEntry>(entries.Count);
        foreach (var entry in entries)
        {
            if (entry is null) throw new InvalidDataException("Invalid departure cache entry.");
            ValidateKey(entry.Key);
            if (!keys.Add(entry.Key)) throw new InvalidDataException("Duplicate departure cache identity.");
            if (entry.Result is null || entry.Result.ErrorCode is not null || entry.Result.IsStale || entry.Result.Items.Count > MaximumEventsPerEntry)
                throw new InvalidDataException("Departure cache must contain a successful bounded result.");
            var lines = entry.Lines ?? [];
            if (TooLong(entry.Result.Source, 256) || entry.Result.Warnings.Count > 32 || entry.Result.Warnings.Any(warning => TooLong(warning, 1024))
                || lines.Count > 100 || lines.Any(line => string.IsNullOrWhiteSpace(line) || line.Length > 256))
                throw new InvalidDataException("Invalid departure cache metadata.");
            var events = entry.Result.Items.Select(WithoutCoordinate).ToArray();
            ValidateEvents(events);
            result.Add(entry with { Result = entry.Result with { Items = events }, Lines = lines.Distinct(StringComparer.Ordinal).OrderBy(line => line, StringComparer.Ordinal).ToArray() });
        }
        return result.ToArray();
    }

    private static DepartureCacheEntry AssertSingle(DepartureCacheEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        return Validate([entry])[0];
    }

    private static void ValidateKey(DepartureCacheKey key)
    {
        ArgumentNullException.ThrowIfNull(key);
        if (string.IsNullOrWhiteSpace(key.Source) || string.IsNullOrWhiteSpace(key.Id) || TooLong(key.Source, 256) || TooLong(key.Id, 1024))
            throw new InvalidDataException("Invalid departure cache identity.");
    }

    private static StopEvent WithoutCoordinate(StopEvent item)
    {
        if (item is null) throw new InvalidDataException("Invalid cached departure.");
        var identity = item.Identity ?? throw new InvalidDataException("Invalid cached departure identity.");
        var stop = identity.Stop ?? throw new InvalidDataException("Invalid cached departure stop.");
        return item with { Identity = identity with { Stop = stop with { Coordinate = null } } };
    }

    private static void ValidateEvents(IReadOnlyList<StopEvent> events)
    {
        foreach (var item in events)
        {
            var identity = item.Identity ?? throw new InvalidDataException("Invalid cached departure identity.");
            var stop = identity.Stop ?? throw new InvalidDataException("Invalid cached departure stop.");
            var realtime = item.Realtime ?? throw new InvalidDataException("Invalid cached departure realtime data.");
            if (stop.Coordinate is not null || TooLong(identity.Source, 256) || OptionalTooLong(identity.TripId, 1024) ||
                OptionalTooLong(identity.SharedTripId, 1024) || OptionalTooLong(identity.Line, 256) || OptionalTooLong(identity.Operator, 1024) || OptionalTooLong(identity.Direction, 1024) ||
                TooLong(stop.Id, 1024) || TooLong(stop.Source, 256) || TooLong(stop.Name, 1024) || OptionalTooLong(stop.Dhid, 1024) ||
                OptionalTooLong(item.Line?.Id, 1024) || OptionalTooLong(item.Line?.Name, 256) || OptionalTooLong(item.Line?.Mode, 128) || OptionalTooLong(item.Line?.Operator?.Id, 1024) || OptionalTooLong(item.Line?.Operator?.Name, 1024) ||
                TooLong(realtime.Source, 256) || OptionalTooLong(realtime.Platform, 256) || OptionalTooLong(realtime.PlannedPlatform, 256))
                throw new InvalidDataException("Invalid cached departure data.");
        }
    }

    private static bool TooLong(string? value, int maximum) => value is null || value.Length > maximum;
    private static bool OptionalTooLong(string? value, int maximum) => value is not null && value.Length > maximum;
}

[JsonSourceGenerationOptions(UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow)]
[JsonSerializable(typeof(DepartureCacheEntry[]))]
internal partial class DepartureCacheJsonContext : JsonSerializerContext;
