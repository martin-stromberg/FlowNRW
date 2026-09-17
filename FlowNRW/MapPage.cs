using System.Text.Json;
using System.Text.Json.Nodes;
using FlowNRW.Core.Maps;
using FlowNRW.Core.Presentation;

namespace FlowNRW;

/// <summary>Local interactive map with native accessible alternatives and a bounded JSON bridge.</summary>
public sealed class MapPage : ContentPage
{
    private readonly MapViewModel model;
    private readonly IMapTileService tiles;
    private readonly MapOptions options;
    private readonly HybridWebView map = new() { HybridRoot = "map", DefaultFile = "index.html", HeightRequest = 430, AutomationId = "MapCanvas" };
    private readonly Label status = new() { Text = "Karte wird geladen …", AutomationId = "MapStatus" };
    private CancellationTokenSource? lifetime;
    private bool ready;
    private int pending;
    private readonly Dictionary<int, CancellationTokenSource> tileRequests = [];
#if UI_TEST_FIXTURES
    private readonly Label viewport = new() { AutomationId = "MapViewport" };
#endif

    /// <summary>Creates the current map snapshot page.</summary>
    /// <param name="model">Selected stop or journey snapshot.</param>
    /// <param name="tiles">Bounded tile gateway.</param>
    /// <param name="options">Provider and projection limits.</param>
    public MapPage(MapViewModel model, IMapTileService tiles, MapOptions options)
    {
        this.model = model; this.tiles = tiles; this.options = options;
        Title = model.Title;
        var layout = new VerticalStackLayout { Padding = 16, Spacing = 12 };
        layout.Children.Add(new Label { Text = model.Status, AutomationId = "MapDataStatus", FontSize = 18 });
        layout.Children.Add(new Label { Text = model.Metadata, AutomationId = "MapMetadata" });
        layout.Children.Add(status);
#if UI_TEST_FIXTURES
        layout.Children.Add(viewport);
#endif
        layout.Children.Add(new Button
        {
            Text = "Karte erneut laden / auf Auswahl zentrieren",
            AutomationId = "ResetMap",
            LineBreakMode = LineBreakMode.WordWrap,
            Command = new AsyncRelayCommand(InitializeAsync, () => true, failure: _ => status.Text = "Karte nicht verfügbar. Bitte die Haltestellenliste verwenden.")
        });
        layout.Children.Add(new Label { Text = options.Attribution, AutomationId = "MapAttribution" });
        layout.Children.Add(new Label { Text = "Kartendaten: Die angezeigte Region wird beim Kartenanbieter abgerufen. Keine Standortfreigabe erforderlich.", FontSize = 13 });
        foreach (var segment in model.Segments)
            layout.Children.Add(new Label { Text = "Verlauf: " + segment.Label + " · " + segment.Points.Count + " gelieferte Punkte", AutomationId = "MapSegment" + model.Segments.ToList().IndexOf(segment) });
        var list = new VerticalStackLayout { Spacing = 12, IsVisible = false };
        if (model.Stations.Count > 0)
        {
            layout.Children.Add(new Button { Text = "Haltestellenliste anzeigen", AutomationId = "ShowMapList", Command = new RelayCommand(_ => { map.IsVisible = false; list.IsVisible = true; }) });
            layout.Children.Add(new Button { Text = "Karte anzeigen", AutomationId = "ShowMapCanvas", Command = new AsyncRelayCommand(async () => { list.IsVisible = false; map.IsVisible = true; await InitializeAsync(); }, () => true, _ => status.Text = "Karte nicht verfügbar.") });
            list.Children.Add(new Label { Text = "Haltestellenliste – Abfahrten öffnen", FontSize = 20, FontAttributes = FontAttributes.Bold });
        }
        var session = model.Session;
        foreach (var station in model.Stations)
            list.Children.Add(new Button
            {
                Text = station.Label,
                LineBreakMode = LineBreakMode.WordWrap,
                AutomationId = "MapStation" + station.Index,
                Command = new AsyncRelayCommand(() => model.SelectAsync(session, station.Index), () => true, failure: _ => status.Text = "Monitor konnte nicht geöffnet werden.")
            });
        layout.Children.Add(list);
        layout.Children.Add(map);
        map.RawMessageReceived += MessageReceived;
        Content = new ScrollView { Content = layout };
    }

    /// <inheritdoc />
    protected override void OnAppearing()
    {
        base.OnAppearing();
        lifetime = new();
        if (ready) _ = InitializeSafelyAsync();
        _ = CheckStartupAsync(lifetime.Token);
    }

    /// <inheritdoc />
    protected override void OnDisappearing()
    {
        lifetime?.Cancel(); lifetime?.Dispose(); lifetime = null;
        base.OnDisappearing();
    }

    private async Task CheckStartupAsync(CancellationToken token)
    {
        try { await Task.Delay(TimeSpan.FromSeconds(15), token); if (!ready) status.Text = "Karte nicht verfügbar. Haltestellenliste und Zurück bleiben nutzbar."; }
        catch (OperationCanceledException) { }
    }

    private async Task InitializeSafelyAsync()
    {
        try { await InitializeAsync(); }
        catch (Exception) { status.Text = "Karte nicht verfügbar. Bitte die Haltestellenliste verwenden."; }
    }

    private Task InitializeAsync()
    {
        if (lifetime is null) return Task.CompletedTask;
        lifetime.Cancel(); lifetime.Dispose(); lifetime = new();
        status.Text = "Basiskarte wird geladen …";
        var markers = new JsonArray();
        foreach (var station in model.Stations.Where(station => station.Position is not null))
            markers.Add(new JsonObject { ["id"] = station.Index, ["label"] = station.Label, ["lat"] = station.Position!.Latitude, ["lon"] = station.Position.Longitude });
        var segments = new JsonArray();
        foreach (var segment in model.Segments)
            segments.Add(new JsonObject { ["label"] = segment.Label, ["points"] = Points(segment.Points) });
        return SendAsync(new JsonObject
        {
            ["type"] = "init",
            ["session"] = model.Session,
            ["markers"] = markers,
            ["segments"] = segments,
            ["endpoints"] = Points(model.Endpoints),
            ["minZoom"] = options.MinZoom,
            ["maxZoom"] = options.MaxZoom
        });
    }

    private static JsonArray Points(IEnumerable<FlowNRW.Core.Transit.GeoCoordinate> points)
    {
        var array = new JsonArray();
        foreach (var point in points) array.Add(new JsonArray(JsonValue.Create(point.Latitude), JsonValue.Create(point.Longitude)));
        return array;
    }

    private Task SendAsync(JsonObject payload) => map.EvaluateJavaScriptAsync("window.receiveMap(" + payload.ToJsonString() + ")");

    private async void MessageReceived(object? sender, HybridWebViewRawMessageReceivedEventArgs args)
    {
        if (lifetime is null || args.Message is not { Length: <= 1024 } message) return;
        var token = lifetime.Token;
        try
        {
            using var document = JsonDocument.Parse(message);
            var root = document.RootElement;
            var type = root.GetProperty("type").GetString();
            if (type == "ready") { ready = true; await InitializeAsync(); return; }
            if (root.GetProperty("session").GetString() != model.Session) return;
#if UI_TEST_FIXTURES
            if (type == "view") { viewport.Text = root.GetProperty("lat").GetRawText() + ";" + root.GetProperty("lon").GetRawText() + ";" + root.GetProperty("zoom").GetRawText(); return; }
#endif
            if (type == "select") { await model.SelectAsync(model.Session, root.GetProperty("id").GetInt32()); return; }
            if (type == "loaded") { status.Text = model.Stations.Count > 0 ? "Basiskarte geladen. Marker auswählen oder Haltestellenliste verwenden." : "Basiskarte geladen. Gelieferte Verläufe können verschoben und vergrößert werden."; return; }
            if (type == "failed") { status.Text = "Basiskarte fehlt oder ist veraltet. Haltestellen und gelieferte Verläufe bleiben nutzbar."; return; }
            if (type == "cancel") { if (tileRequests.TryGetValue(root.GetProperty("id").GetInt32(), out var previous)) previous.Cancel(); return; }
            if (type != "tile") return;
            var id = root.GetProperty("id").GetInt32();
            var z = root.GetProperty("z").GetInt32(); var x = root.GetProperty("x").GetInt32(); var y = root.GetProperty("y").GetInt32();
            if (pending >= 64 || tileRequests.ContainsKey(id)) return;
            pending++;
            using var tileLifetime = CancellationTokenSource.CreateLinkedTokenSource(token);
            tileRequests[id] = tileLifetime;
            try
            {
                var tile = await tiles.GetAsync(z, x, y, tileLifetime.Token);
                if (!tileLifetime.IsCancellationRequested)
                    await SendAsync(new JsonObject { ["type"] = "tile", ["session"] = model.Session, ["id"] = id, ["data"] = tile.Data, ["error"] = tile.Error });
            }
            finally { pending--; tileRequests.Remove(id); }
        }
        catch (OperationCanceledException) { }
        catch (Exception) { if (!token.IsCancellationRequested) status.Text = "Kartenabruf fehlgeschlagen. Bitte erneut laden oder die Liste verwenden."; }
    }
}
