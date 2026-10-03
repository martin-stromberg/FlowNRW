using System.Text.Json;
using System.Text.Json.Nodes;
using FlowNRW.Core.Maps;
using FlowNRW.Core.Presentation;
using FlowNRW.Core.Refresh;
using System.ComponentModel;

namespace FlowNRW;

/// <summary>Local interactive map with native accessible alternatives and a bounded JSON bridge.</summary>
public sealed class MapPage : ContentPage
{
    private readonly MapViewModel model;
    private readonly IMapTileService tiles;
    private readonly MapOptions options;
    private readonly ForegroundState foreground;
    private readonly HybridWebView map = new() { HybridRoot = "map", DefaultFile = "index.html", HeightRequest = 430, AutomationId = "MapCanvas" };
    private readonly Label status = new() { Text = "Karte wird geladen …", AutomationId = "MapStatus", FontSize = 15 };
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
    /// <param name="foreground">Shared active-window state.</param>
    public MapPage(MapViewModel model, IMapTileService tiles, MapOptions options, ForegroundState foreground)
    {
        this.foreground = foreground;
        this.model = model; this.tiles = tiles; this.options = options;
        Title = model.Title;
        var layout = new VerticalStackLayout { Padding = 16, Spacing = 12 };
        layout.Children.Add(TransitVisuals.Text(model.Title, 28, true));
        var mapState = new VerticalStackLayout { Spacing = 4, Padding = 16 };
        mapState.Children.Add(new Label { Text = model.Status.Replace(". Auswahl öffnet die Abfahrten.", "", StringComparison.Ordinal).Replace("Es wird kein Streckenverlauf erfunden.", "Für diese Verbindung ist kein Streckenverlauf verfügbar.", StringComparison.Ordinal), AutomationId = "MapDataStatus", FontSize = 17 });
        mapState.Children.Add(status);
        layout.Children.Add(new Border { Content = mapState });
#if UI_TEST_FIXTURES
        viewport.IsVisible = Environment.GetEnvironmentVariable("FLOWNRW_UI_TEST_HIDE_CONTROLS") != "1";
        layout.Children.Add(viewport);
#endif
        var reset = TransitVisuals.SecondaryAction("↻", "Karte neu laden", "ResetMap",
            new AsyncRelayCommand(InitializeAsync, () => true, failure: _ => status.Text = "Karte nicht verfügbar. Bitte die Haltestellenliste verwenden."));
        SizeChanged += (_, _) => map.HeightRequest = Math.Max(320, Height * 0.55);
        var information = new VerticalStackLayout { Spacing = 8, IsVisible = false };
        information.Children.Add(new Label { Text = model.Metadata, AutomationId = "MapMetadata", FontSize = 13 });
        information.Children.Add(new Label { Text = "Kartendaten: Die angezeigte Region wird beim Kartenanbieter abgerufen. Keine Standortfreigabe erforderlich.", FontSize = 13 });
        foreach (var segment in model.Segments)
            information.Children.Add(new Label { Text = "Verlauf: " + segment.Label + " · " + segment.Points.Count + " gelieferte Punkte", AutomationId = "MapSegment" + model.Segments.ToList().IndexOf(segment), FontSize = 13 });
        var infoButton = new Button { Text = "ⓘ", AutomationId = "MapInformation", HeightRequest = 48, WidthRequest = 48, Padding = new Thickness(0) };
        SemanticProperties.SetDescription(infoButton, "Quellen und Kartendaten");
        infoButton.Command = new RelayCommand(_ =>
        {
            information.IsVisible = !information.IsVisible;
            SemanticProperties.SetDescription(infoButton, information.IsVisible ? "Kartendaten schließen" : "Quellen und Kartendaten");
        });
        var actions = new Grid { ColumnDefinitions = [new(GridLength.Auto), new(GridLength.Auto), new(GridLength.Star)], ColumnSpacing = 8 };
        actions.Add(reset, 0); actions.Add(infoButton, 1);
        var list = new VerticalStackLayout { Spacing = 12, IsVisible = false };
        if (model.Stations.Count > 0)
        {
            var listButton = new Button { Text = "Liste", AutomationId = "ShowMapList", Command = new RelayCommand(_ => { map.IsVisible = false; list.IsVisible = true; }) };
            var canvasButton = new Button { Text = "Karte", AutomationId = "ShowMapCanvas", Command = new AsyncRelayCommand(async () => { list.IsVisible = false; map.IsVisible = true; await InitializeAsync(); }, () => true, _ => status.Text = "Karte nicht verfügbar.") };
            actions.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            actions.Add(listButton, 2); actions.Add(canvasButton, 3);
            list.Children.Add(TransitVisuals.Text("Haltestellenliste", 20, true));
        }
        var session = model.Session;
        foreach (var station in model.Stations)
            list.Children.Add(TransitVisuals.Candidate(station.Candidate, new Button
            {
                Text = station.Label,
                LineBreakMode = LineBreakMode.WordWrap,
                AutomationId = "MapStation" + station.Index,
                Command = new AsyncRelayCommand(() => model.SelectAsync(session, station.Index), () => true, failure: _ => status.Text = "Monitor konnte nicht geöffnet werden.")
            }, station.Position is null ? " · Keine Kartenposition vorhanden" : ""));
        layout.Children.Add(actions);
        layout.Children.Add(map);
        layout.Children.Add(new Label { Text = options.Attribution, AutomationId = "MapAttribution", FontSize = 13 });
        layout.Children.Add(information);
        layout.Children.Add(list);
        map.RawMessageReceived += MessageReceived;
        Content = TransitVisuals.Page(layout);
    }

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
        if (lifetime is null || !foreground.IsActive) return Task.CompletedTask;
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
            if (token.IsCancellationRequested || !foreground.IsActive) return;
            if (root.GetProperty("session").GetString() != model.Session) return;
#if UI_TEST_FIXTURES
            if (type == "view") { viewport.Text = root.GetProperty("lat").GetRawText() + ";" + root.GetProperty("lon").GetRawText() + ";" + root.GetProperty("zoom").GetRawText(); return; }
#endif
            if (type == "select") { await model.SelectAsync(model.Session, root.GetProperty("id").GetInt32()); return; }
            if (type == "loaded") { status.Text = "Basiskarte geladen."; return; }
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
