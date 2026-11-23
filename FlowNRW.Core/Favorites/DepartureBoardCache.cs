using FlowNRW.Core.Refresh;
using FlowNRW.Core.Transit;

namespace FlowNRW.Core.Favorites;

/// <summary>Single read facade restoring persisted departure boards of saved favorite stops for both the home cards and the stop monitor.</summary>
public sealed class DepartureBoardCache
{
    private readonly IDepartureCacheStore store;
    private readonly IFavoriteStore favorites;
    private readonly RefreshFreshness freshness;
    private readonly SemaphoreSlim gate = new(1, 1);

    /// <summary>Creates the shared restore facade without accessing storage.</summary>
    /// <param name="store">Bounded durable cache of last-known favorite boards.</param>
    /// <param name="favorites">Saved favorite identities authorizing persistent reads.</param>
    /// <param name="freshness">Policy deciding whether a durable board is marked stale.</param>
    public DepartureBoardCache(IDepartureCacheStore store, IFavoriteStore favorites, RefreshFreshness? freshness = null)
    {
        this.store = store;
        this.favorites = favorites;
        this.freshness = freshness ?? new RefreshFreshness(new TransitCacheOptions());
    }

    /// <summary>Restores the persisted board of one saved favorite for the monitor path.</summary>
    /// <param name="stop">Complete candidate identity.</param>
    /// <param name="cancellationToken">Cancels the bundled favorite and cache read.</param>
    /// <returns>The projected board, or null when the stop is not a saved favorite or has no cached board.</returns>
    public async Task<RestoredDepartureBoard?> RestoreBoardAsync(Stop stop, CancellationToken cancellationToken = default)
    {
        await gate.WaitAsync(cancellationToken);
        try
        {
            var saved = await favorites.LoadAsync(cancellationToken).WaitAsync(cancellationToken);
            if (!saved.Any(item => item.Source == stop.Source && item.Id == stop.Id)) return null;
            var entries = await store.LoadAsync(cancellationToken).WaitAsync(cancellationToken);
            var entry = entries.FirstOrDefault(item => item.Source == stop.Source && item.StopId == stop.Id);
            return entry is null ? null : Project(entry);
        }
        finally { gate.Release(); }
    }

    /// <summary>Restores the persisted boards of the supplied favorite keys for the home path.</summary>
    /// <param name="keys">Authoritative saved favorite identities.</param>
    /// <param name="cancellationToken">Cancels the bundled cache read.</param>
    /// <returns>Projected boards keyed by their favorite identity; unrelated cache entries are not projected.</returns>
    public async Task<IReadOnlyDictionary<DepartureCacheKey, RestoredDepartureBoard>> RestoreBoardsAsync(IReadOnlyCollection<DepartureCacheKey> keys, CancellationToken cancellationToken = default)
    {
        await gate.WaitAsync(cancellationToken);
        try
        {
            var entries = await store.LoadAsync(cancellationToken).WaitAsync(cancellationToken);
            var restored = new Dictionary<DepartureCacheKey, RestoredDepartureBoard>();
            foreach (var entry in entries)
            {
                if (keys.Contains(entry.Key)) restored[entry.Key] = Project(entry);
            }
            return restored;
        }
        finally { gate.Release(); }
    }

    private RestoredDepartureBoard Project(DepartureCacheEntry entry)
    {
        var now = DateTimeOffset.Now;
        var storedLines = entry.Lines ?? [];
        var lines = SortLines(storedLines.Count > 0 ? storedLines : entry.Result.Items.Select(LineName));
        var retained = entry.Result.Items.Where(item => EffectiveTime(item) is { } time && time >= now)
            .OrderBy(item => EffectiveTime(item)!.Value).ToArray();
        return new RestoredDepartureBoard
        {
            Result = retained.Length == 0 ? null
                : entry.Result with { Items = retained, IsStale = entry.Result.IsStale || freshness.IsStale(entry.Result) },
            Lines = lines
        };
    }

    /// <summary>Effective departure instant: actual realtime time, or planned time plus a known delay.</summary>
    /// <param name="item">Normalized stop event.</param>
    /// <returns>The effective instant, or null for timeless events.</returns>
    internal static DateTimeOffset? EffectiveTime(StopEvent item) => item.Realtime.ActualTime
        ?? (item.PlannedTime is { } planned ? planned + (item.Realtime.Delay ?? TimeSpan.Zero) : null);

    /// <summary>Public line name from the line object or the trip identity.</summary>
    /// <param name="item">Normalized stop event.</param>
    /// <returns>The line name, or an empty string.</returns>
    internal static string LineName(StopEvent item) => item.Line?.Name ?? item.Identity.Line ?? string.Empty;

    /// <summary>Non-empty distinct line names in ordinal order.</summary>
    /// <param name="lines">Raw line names.</param>
    /// <returns>The sorted unique line inventory.</returns>
    internal static string[] SortLines(IEnumerable<string> lines)
    {
        return lines.Where(line => !string.IsNullOrWhiteSpace(line))
            .Distinct(StringComparer.Ordinal).OrderBy(line => line, StringComparer.Ordinal).ToArray();
    }
}

/// <summary>Projected persisted board: filtered and stale-marked result plus the retained line inventory.</summary>
public sealed record RestoredDepartureBoard
{
    /// <summary>Filtered future departures already marked stale when applicable, or null when no displayable board remains.</summary>
    public ProviderResult<StopEvent>? Result { get; init; }

    /// <summary>Sorted line inventory preserved even without a displayable board.</summary>
    public IReadOnlyList<string> Lines { get; init; } = [];
}
