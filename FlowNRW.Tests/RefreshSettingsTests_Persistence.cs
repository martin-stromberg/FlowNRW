using FlowNRW.Core.Refresh;

namespace FlowNRW.Tests;

/// <summary>Settings persistence never activates drafts or silently overwrites a bad file.</summary>
public sealed class RefreshSettingsTests_Persistence
{
    /// <summary>Every supported choice survives a distinct store and model instance.</summary>
    /// <param name="seconds">Supported interval.</param>
    [Theory]
    [InlineData(0)]
    [InlineData(30)]
    [InlineData(60)]
    [InlineData(120)]
    [InlineData(300)]
    public async Task SupportedChoicesSurviveRestart(int seconds)
    {
        using var files = new RefreshTestFiles();
        await new JsonRefreshSettingsStore(files.Path).SaveAsync(seconds);
        var model = new RefreshSettingsViewModel(new JsonRefreshSettingsStore(files.Path));
        await model.LoadAsync();
        Assert.True(model.IsLoaded); Assert.Equal(seconds, model.IntervalSeconds); Assert.Equal(seconds, model.SelectedSeconds);
        if (seconds == 0) Assert.Contains("Aus", model.Description);
        Assert.Equal(seconds.ToString(System.Globalization.CultureInfo.InvariantCulture), await File.ReadAllTextAsync(files.Path));
    }

    /// <summary>Invalid or oversized saved data remains untouched until an explicit successful save.</summary>
    /// <param name="content">Invalid JSON or invalid interval.</param>
    [Theory]
    [InlineData("broken")]
    [InlineData("null")]
    [InlineData("[]")]
    [InlineData("{\"seconds\":30}")]
    [InlineData("31")]
    [InlineData("-1")]
    [InlineData("30.5")]
    [InlineData("300000000000000000000000000000000000000000000000000000000000000000000000")]
    public async Task CorruptSettingsUseSafeDefaultWithoutOverwriting(string content)
    {
        using var files = new RefreshTestFiles(); await File.WriteAllTextAsync(files.Path, content);
        var store = new JsonRefreshSettingsStore(files.Path);
        await Assert.ThrowsAsync<InvalidDataException>(() => store.LoadAsync());
        var model = new RefreshSettingsViewModel(store); var changes = 0; model.Changed += (_, _) => changes++;
        await model.LoadAsync();
        Assert.True(model.IsLoaded); Assert.Equal(60, model.IntervalSeconds); Assert.Contains("nicht geladen", model.Status);
        Assert.Equal(content, await File.ReadAllTextAsync(files.Path)); Assert.Equal(0, changes);
        model.SelectedSeconds = 120; await model.SaveAsync();
        Assert.Equal(120, await new JsonRefreshSettingsStore(files.Path).LoadAsync()); Assert.Equal(1, changes);
    }

    /// <summary>Missing settings resolve the documented default without creating a file.</summary>
    [Fact]
    public async Task MissingFileResolvesDefaultAndLoadsOnce()
    {
        using var files = new RefreshTestFiles(); var model = new RefreshSettingsViewModel(new JsonRefreshSettingsStore(files.Path));
        Assert.False(model.IsLoaded); await model.LoadAsync();
        Assert.True(model.IsLoaded); Assert.Equal(60, model.IntervalSeconds); Assert.False(File.Exists(files.Path));
        await new JsonRefreshSettingsStore(files.Path).SaveAsync(0); await model.LoadAsync();
        Assert.Equal(60, model.IntervalSeconds);
    }

    /// <summary>Cancelled writes leave prior durable data and no temporary files.</summary>
    [Fact]
    public async Task CancelledSavePreservesPreviousBytes()
    {
        using var files = new RefreshTestFiles(); var store = new JsonRefreshSettingsStore(files.Path);
        await store.SaveAsync(120); var previous = await File.ReadAllBytesAsync(files.Path);
        using var cancellation = new CancellationTokenSource(); cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => store.SaveAsync(30, cancellation.Token));
        Assert.Equal(previous, await File.ReadAllBytesAsync(files.Path));
        Assert.Single(Directory.GetFiles(files.Directory));
    }

    /// <summary>Failed atomic replacement does not damage the target and removes its temporary file.</summary>
    [Fact]
    public async Task FailedReplacementPreservesTargetAndCleansTemporaryFile()
    {
        using var files = new RefreshTestFiles(); Directory.CreateDirectory(files.Path);
        var marker = System.IO.Path.Combine(files.Path, "existing.txt"); await File.WriteAllTextAsync(marker, "keep");
        var failure = await Record.ExceptionAsync(() => new JsonRefreshSettingsStore(files.Path).SaveAsync(30));
        Assert.True(failure is IOException or UnauthorizedAccessException);
        Assert.Equal("keep", await File.ReadAllTextAsync(marker));
        Assert.Empty(Directory.GetFiles(files.Directory));
    }

    /// <summary>A failed save leaves the effective interval unchanged and retry raises Changed once.</summary>
    [Fact]
    public async Task FailedDraftSaveCanBeRetriedWithoutPrematureChange()
    {
        var store = new ControlledRefreshSettingsStore { Loaded = 120 };
        var model = new RefreshSettingsViewModel(store); await model.LoadAsync();
        var changes = 0; model.Changed += (_, _) => changes++;
        model.SelectedSeconds = 30; Assert.Equal(120, model.IntervalSeconds);
        var saving = model.SaveAsync(); await model.SaveAsync();
        Assert.True(model.IsSaving); Assert.Single(store.Saves); Assert.Equal(0, changes);
        store.Saves[0].SetException(new IOException("sensitive path")); await saving;
        Assert.Equal(120, model.IntervalSeconds); Assert.False(model.IsSaving); Assert.Equal(0, changes);
        Assert.Contains("nicht gespeichert", model.Status); Assert.DoesNotContain("sensitive", model.Status);
        saving = model.SaveAsync(); store.Saves[1].SetResult(); await saving;
        Assert.Equal(30, model.IntervalSeconds); Assert.Equal(1, changes);
    }

    /// <summary>Unsupported drafts never reach persistence or change the active timer setting.</summary>
    /// <param name="seconds">Invalid draft.</param>
    [Theory]
    [InlineData(-1)]
    [InlineData(1)]
    [InlineData(29)]
    [InlineData(31)]
    [InlineData(301)]
    [InlineData(int.MaxValue)]
    public async Task InvalidDraftIsRejectedBeforeSave(int seconds)
    {
        var store = new ControlledRefreshSettingsStore(); var model = new RefreshSettingsViewModel(store);
        await model.LoadAsync(); model.SelectedSeconds = seconds; await model.SaveAsync();
        Assert.Empty(store.Saves); Assert.Equal(60, model.IntervalSeconds); Assert.False(model.IsSaving);
        Assert.Contains("auswählen", model.Status);
    }
}

internal sealed class ControlledRefreshSettingsStore : IRefreshSettingsStore
{
    internal int? Loaded { get; init; }
    internal List<TaskCompletionSource> Saves { get; } = [];
    public Task<int?> LoadAsync(CancellationToken cancellationToken = default) => Task.FromResult(Loaded);
    public Task SaveAsync(int seconds, CancellationToken cancellationToken = default)
    {
        var source = new TaskCompletionSource(); Saves.Add(source); return source.Task;
    }
}

internal sealed class RefreshTestFiles : IDisposable
{
    internal string Directory { get; } = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "flownrw-refresh-" + Guid.NewGuid().ToString("N"));
    internal string Path => System.IO.Path.Combine(Directory, "settings.json");
    internal RefreshTestFiles() { System.IO.Directory.CreateDirectory(Directory); }
    public void Dispose() => System.IO.Directory.Delete(Directory, true);
}
