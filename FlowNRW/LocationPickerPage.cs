using System.Text.Json;
using System.Text.Json.Nodes;
using FlowNRW.Core.Maps;
using FlowNRW.Core.Presentation;
using FlowNRW.Core.Refresh;
using FlowNRW.Core.Transit;
using System.ComponentModel;

namespace FlowNRW;

/// <summary>OpenStreetMap position picker sharing the bounded tile bridge with the map page.</summary>
public sealed class LocationPickerPage : ContentPage
{
    private readonly MapOptions options;
    private readonly ForegroundState foreground;
    private readonly MapTileBridge bridge;
    private readonly HybridWebView map = new() { HybridRoot = "map", DefaultFile = "index.html", HeightRequest = 430, AutomationId = "PickerMapCanvas" };
    private readonly Label status = new() { Text = "Karte wird geladen …", AutomationId = "PickerStatus", FontSize = 15 };
    private readonly Button apply;
    private readonly TaskCompletionSource<GeoCoordinate?> completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private CancellationTokenSource? lifetime;
    private bool ready;
    private bool loadSettled;
    private int loadId;
    private string session = "";
    private GeoCoordinate? picked;

    /// <summary>Creates the modal picker.</summary>
    /// <param name="tiles">Bounded tile gateway.</param>
    /// <param name="options">Provider and projection limits.</param>
    /// <param name="foreground">Shared active-window state.</param>
    /// <param name="initial">Optional position to focus and preselect.</param>
    /// <param name="endpoint">Accessible endpoint name shown in the heading.</param>
    public LocationPickerPage(IMapTileService tiles, MapOptions options, ForegroundState foreground, GeoCoordinate? initial = null, string endpoint = "Position")
    {
        this.options = options;
        this.foreground = foreground;
        bridge = new MapTileBridge(tiles);
        picked = MapViewModel.IsMappable(initial) ? initial : null;
        Title = "Position wählen";
        var layout = new VerticalStackLayout { Padding = 16, Spacing = 12 };
        layout.Children.Add(TransitVisuals.Text(endpoint + ": Position auf der Karte wählen", 20, true));
        var mapState = new VerticalStackLayout { Spacing = 4, Padding = 16 };
        mapState.Children.Add(status);
        layout.Children.Add(new Border { Content = mapState });
        apply = new Button { Text = "Übernehmen", AutomationId = "PickerApply", IsEnabled = picked is not null };
        SemanticProperties.SetDescription(apply, "Gewählte Position übernehmen");
        apply.Command = new RelayCommand(_ => _ = FinishAsync(picked));
        var cancel = new Button { Text = "Abbrechen", AutomationId = "PickerCancel" };
        SemanticProperties.SetDescription(cancel, "Positionsauswahl abbrechen");
        cancel.Command = new RelayCommand(_ => _ = FinishAsync(null));
        var reload = TransitVisuals.SecondaryAction(TransitIcons.Refresh, "Karte neu laden", "PickerReload",
            new AsyncRelayCommand(InitializeAsync, () => true, failure: _ => Fail("Karte nicht verfügbar. Die Position kann nicht gewählt werden.")));
        var actions = new Grid { ColumnDefinitions = [new(GridLength.Star), new(GridLength.Star), new(GridLength.Auto)], ColumnSpacing = 8 };
        actions.Add(cancel, 0); actions.Add(apply, 1); actions.Add(reload, 2);
        layout.Children.Add(actions);
        SizeChanged += (_, _) => map.HeightRequest = Math.Max(320, Height * 0.55);
        layout.Children.Add(map);
        layout.Children.Add(new Label { Text = options.Attribution, AutomationId = "PickerAttribution", FontSize = 13 });
        map.RawMessageReceived += MessageReceived;
        Content = TransitVisuals.Page(layout);
    }

    /// <summary>Completes with the picked coordinate, or <see langword="null"/> when dismissed.</summary>
    public Task<GeoCoordinate?> Selection => completion.Task;

    /// <inheritdoc />
    protected override void OnAppearing()
    {
        base.OnAppearing();
        foreground.PropertyChanged += ForegroundChanged;
        lifetime = new();
        if (ready) _ = InitializeSafelyAsync();
        _ = CheckStartupAsync(lifetime.Token);
    }

    /// <inheritdoc />
    protected override void OnDisappearing()
    {
        foreground.PropertyChanged -= ForegroundChanged;
        lifetime?.Cancel(); lifetime?.Dispose(); lifetime = null;
        bridge.CancelAll();
        completion.TrySetResult(null);
        base.OnDisappearing();
    }

    private void ForegroundChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (!foreground.IsActive)
        {
            lifetime?.Cancel();
            status.Text = "Kartenabrufe pausiert. Beim Zurückkehren wird die Karte neu geladen.";
        }
        else
        {
            lifetime?.Dispose(); lifetime = new();
            if (ready) _ = InitializeSafelyAsync();
            else _ = CheckStartupAsync(lifetime.Token);
        }
    }

    private async Task CheckStartupAsync(CancellationToken token)
    {
        try { await Task.Delay(TimeSpan.FromSeconds(15), token); if (!ready) Fail("Karte nicht verfügbar. Die Position kann nicht gewählt werden."); }
        catch (OperationCanceledException) { }
    }

    private async Task InitializeSafelyAsync()
    {
        try { await InitializeAsync(); }
        catch (Exception) { Fail("Karte nicht verfügbar. Die Position kann nicht gewählt werden."); }
    }

    private void Fail(string message)
    {
        status.Text = message;
        apply.IsEnabled = false;
    }

    private async Task InitializeAsync()
    {
        if (lifetime is null || !foreground.IsActive) return;
        lifetime.Cancel(); lifetime.Dispose(); lifetime = new();
        var token = lifetime.Token;
        session = Guid.NewGuid().ToString("N");
        var generation = ++loadId;
        bridge.CancelAll();
        loadSettled = false;
        apply.IsEnabled = picked is not null;
        status.Text = "Basiskarte wird geladen …";
        var center = picked ?? new GeoCoordinate(51.4556, 7.0116);
        var payload = new JsonObject
        {
            ["type"] = "init",
            ["session"] = session,
            ["loadId"] = generation,
            ["picker"] = true,
            ["center"] = new JsonArray(center.Latitude, center.Longitude),
            ["zoom"] = picked is not null ? 15 : 11,
            ["markers"] = new JsonArray(),
            ["segments"] = new JsonArray(),
            ["endpoints"] = new JsonArray(),
            ["minZoom"] = options.MinZoom,
            ["maxZoom"] = options.MaxZoom
        };
        if (picked is not null) payload["picked"] = new JsonArray(picked.Latitude, picked.Longitude);
        await SendAsync(payload);
        _ = WatchLoadAsync(session, generation, token);
    }

    private async Task WatchLoadAsync(string current, int generation, CancellationToken token)
    {
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(30), token);
            if (loadSettled || generation != loadId || current != session || !foreground.IsActive) return;
            Fail("Basiskarte konnte nicht geladen werden. Die Position kann nicht gewählt werden.");
            bridge.CancelAll();
        }
        catch (OperationCanceledException) { }
        catch (Exception) { }
    }

    private Task SendAsync(JsonObject payload) => map.EvaluateJavaScriptAsync("window.receiveMap(" + payload.ToJsonString() + ")");

    private async Task FinishAsync(GeoCoordinate? coordinate)
    {
        completion.TrySetResult(coordinate);
        await Navigation.PopAsync();
    }

    private async void MessageReceived(object? sender, HybridWebViewRawMessageReceivedEventArgs args)
    {
        if (lifetime is null || args.Message is not { Length: <= 1024 } message) return;
        var token = lifetime.Token;
        try
        {
            using var document = JsonDocument.Parse(message);
            var root = document.RootElement;
            var type = root.GetProperty("type").GetString();
            if (type == "ready") { ready = true; await InitializeSafelyAsync(); return; }
            if (token.IsCancellationRequested || !foreground.IsActive) return;
            if (root.GetProperty("session").GetString() != session) return;
            if (type == "pick")
            {
                picked = new GeoCoordinate(root.GetProperty("lat").GetDouble(), root.GetProperty("lon").GetDouble());
                apply.IsEnabled = true;
                status.Text = "Position gewählt. Übernehmen oder einen anderen Punkt antippen.";
                return;
            }
            if (type == "loaded" || type == "failed")
            {
                if (!root.TryGetProperty("loadId", out var generation) || generation.ValueKind != JsonValueKind.Number || generation.GetInt32() != loadId) return;
                loadSettled = true;
                if (type == "loaded")
                {
                    status.Text = "Position auf der Karte antippen.";
                    apply.IsEnabled = picked is not null;
                }
                else Fail("Basiskarte fehlt oder ist veraltet. Die Position kann nicht zuverlässig gewählt werden.");
                return;
            }
            if (type is "tile" or "cancel") await bridge.HandleAsync(root, session, SendAsync, token);
        }
        catch (OperationCanceledException) { }
        catch (Exception) { if (!token.IsCancellationRequested) Fail("Kartenabruf fehlgeschlagen. Bitte erneut laden."); }
    }
}
