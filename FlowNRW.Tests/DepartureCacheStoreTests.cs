using FlowNRW.Core.Favorites;
using FlowNRW.Core.Transit;

namespace FlowNRW.Tests;

/// <summary>Durable, bounded and location-free favorite departure cache behavior.</summary>
public sealed class DepartureCacheStoreTests
{
    /// <summary>Successful boards round-trip by complete provider key and discard event coordinates.</summary>
    [Fact]
    public async Task RoundTrip_SeparatesProviderKeysAndDropsCoordinates()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");
        try
        {
            var store = new JsonDepartureCacheStore(path);
            await store.SaveAsync([Entry("first", "same", 51), Entry("second", "same", 52)]);

            var restored = await store.LoadAsync();

            Assert.Collection(restored,
                first => Assert.Equal("first", first.Source),
                second => Assert.Equal("second", second.Source));
            Assert.Equal("same", restored[0].StopId);
            Assert.Equal("fixture", restored[0].Result.Source);
            var departure = Assert.Single(restored[0].Result.Items);
            Assert.Null(departure.Identity.Stop.Coordinate);
            Assert.Equal("Ziel first", departure.Identity.Direction);
            Assert.DoesNotContain("Latitude", await File.ReadAllTextAsync(path));
        }
        finally { File.Delete(path); }
    }

    /// <summary>Malformed, oversized and semantically failed cache data remains unreadable and cannot be overwritten.</summary>
    /// <param name="original">Malformed or empty on-disk cache content.</param>
    [Theory]
    [InlineData("{broken")]
    [InlineData("[]")]
    public async Task InvalidExistingFileCannotBeOverwritten(string original)
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");
        try
        {
            await File.WriteAllTextAsync(path, original);
            var store = new JsonDepartureCacheStore(path);
            if (original == "[]")
            {
                await store.SaveAsync([Entry("source", "id", 51)]);
                Assert.Single(await store.LoadAsync());
            }
            else
            {
                await Assert.ThrowsAsync<InvalidDataException>(() => store.LoadAsync());
                await Assert.ThrowsAsync<InvalidDataException>(() => store.SaveAsync([Entry("source", "id", 51)]));
                Assert.Equal(original, await File.ReadAllTextAsync(path));
            }
        }
        finally { File.Delete(path); }
    }

    /// <summary>An oversized existing cache is preserved and rejected before a replacement can be written.</summary>
    [Fact]
    public async Task OversizedExistingFileCannotBeOverwritten()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");
        var original = new string(' ', 512 * 1024 + 1);
        try
        {
            await File.WriteAllTextAsync(path, original);
            var store = new JsonDepartureCacheStore(path);
            await Assert.ThrowsAsync<InvalidDataException>(() => store.LoadAsync());
            await Assert.ThrowsAsync<InvalidDataException>(() => store.SaveAsync([Entry("source", "id", 51)]));
            Assert.Equal(original, await File.ReadAllTextAsync(path));
        }
        finally { File.Delete(path); }
    }

    /// <summary>Failed results and duplicate or incomplete technical identities are rejected before persistence.</summary>
    [Fact]
    public async Task Save_InvalidEntriesAreRejected()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");
        try
        {
            var store = new JsonDepartureCacheStore(path);
            await Assert.ThrowsAsync<InvalidDataException>(() => store.SaveAsync([Entry("source", "id", 51) with
            {
                Result = new ProviderResult<StopEvent> { ErrorCode = "offline" }
            }]));
            await Assert.ThrowsAsync<InvalidDataException>(() => store.SaveAsync([Entry("source", "id", 51), Entry("source", "id", 52)]));
            await Assert.ThrowsAsync<InvalidDataException>(() => store.SaveAsync([Entry("", "id", 51)]));
            Assert.False(File.Exists(path));
        }
        finally { File.Delete(path); }
    }

    /// <summary>Cleanup keeps only cached boards belonging to current favorite identities.</summary>
    [Fact]
    public async Task RemoveOrphans_KeepsOnlyCurrentFavoriteKeys()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");
        try
        {
            var store = new JsonDepartureCacheStore(path);
            await store.SaveAsync([Entry("keep", "one", 51), Entry("orphan", "two", 52)]);

            await store.RemoveOrphansAsync([new("keep", "one")]);

            var entry = Assert.Single(await store.LoadAsync());
            Assert.Equal(new DepartureCacheKey("keep", "one"), entry.Key);
            await store.RemoveAsync(entry.Key);
            Assert.Empty(await store.LoadAsync());
        }
        finally { File.Delete(path); }
    }

    /// <summary>An upsert replaces only its own provider-scoped board.</summary>
    [Fact]
    public async Task Upsert_ReplacesOnlyMatchingBoard()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");
        try
        {
            var store = new JsonDepartureCacheStore(path);
            await store.SaveAsync([Entry("first", "same", 51), Entry("second", "same", 52)]);
            await store.UpsertAsync(Entry("first", "same", 53) with { Result = new ProviderResult<StopEvent> { Source = "replacement", Items = [] } });

            var restored = await store.LoadAsync();
            Assert.Collection(restored.OrderBy(entry => entry.Source),
                first => Assert.Equal("replacement", first.Result.Source),
                second => Assert.Equal("fixture", second.Result.Source));
        }
        finally { File.Delete(path); }
    }

    private static DepartureCacheEntry Entry(string source, string id, double latitude) => new()
    {
        Source = source,
        StopId = id,
        Result = new ProviderResult<StopEvent>
        {
            Source = "fixture",
            RetrievedAt = DateTimeOffset.UtcNow,
            Items = [new StopEvent
            {
                PlannedTime = DateTimeOffset.UtcNow.AddMinutes(5),
                Identity = new TripIdentity
                {
                    Source = source,
                    Stop = new Stop { Id = id, Source = source, Name = "Stop " + source, Coordinate = new GeoCoordinate(latitude, 7) },
                    Direction = "Ziel " + source
                },
                Realtime = new RealtimeStatus { Delay = TimeSpan.Zero },
                Line = new Line { Name = "RE 1", Mode = "rail", Operator = new Operator { Name = "Fixture" } }
            }]
        }
    };
}
