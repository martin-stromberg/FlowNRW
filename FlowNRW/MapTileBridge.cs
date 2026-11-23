using System.Text.Json;
using System.Text.Json.Nodes;
using FlowNRW.Core.Maps;

namespace FlowNRW;

/// <summary>Serves bounded tile and cancellation messages of the shared map bridge to one page.</summary>
/// <param name="tiles">Bounded tile gateway.</param>
internal sealed class MapTileBridge(IMapTileService tiles)
{
    private int pending;
    private readonly Dictionary<int, CancellationTokenSource> requests = [];

    /// <summary>Currently in-flight tile requests.</summary>
    internal int Pending => pending;

    /// <summary>Cancels all outstanding tile requests.</summary>
    internal void CancelAll()
    {
        foreach (var request in requests.Values)
        {
            try { request.Cancel(); }
            catch (Exception) { }
        }
        requests.Clear();
    }

    /// <summary>Handles a tile or cancel message; other message types are left to the page.</summary>
    /// <param name="root">Parsed bridge message.</param>
    /// <param name="session">Expected map session identity.</param>
    /// <param name="sendAsync">Replies to the WebView.</param>
    /// <param name="token">Page lifetime.</param>
    /// <param name="log">Optional sanitized diagnostic sink.</param>
    /// <returns>Whether the message was consumed.</returns>
    internal async Task<bool> HandleAsync(JsonElement root, string session, Func<JsonObject, Task> sendAsync,
        CancellationToken token, Action<string>? log = null)
    {
        var type = root.GetProperty("type").GetString();
        if (type == "cancel")
        {
            if (requests.TryGetValue(root.GetProperty("id").GetInt32(), out var previous)) previous.Cancel();
            return true;
        }
        if (type != "tile") return false;
        var id = root.GetProperty("id").GetInt32();
        var z = root.GetProperty("z").GetInt32(); var x = root.GetProperty("x").GetInt32(); var y = root.GetProperty("y").GetInt32();
        if (pending >= 64 || requests.ContainsKey(id))
        {
            // A dropped request is a real failure for this tile: answering with
            // error:true lets the JS layer finish it on the error path and report
            // 'failed' instead of announcing 'loaded' for a map whose tiles are
            // permanently missing (a saturated 'pending' means hung requests).
            log?.Invoke("tile dropped id=" + id + " pending=" + pending);
            await sendAsync(new JsonObject { ["type"] = "tile", ["session"] = session, ["id"] = id, ["data"] = "", ["error"] = true });
            return true;
        }
        pending++;
        using var tileLifetime = CancellationTokenSource.CreateLinkedTokenSource(token);
        requests[id] = tileLifetime;
        try
        {
            var tile = await tiles.GetAsync(z, x, y, tileLifetime.Token);
            if (tile.Error) log?.Invoke("tile error id=" + id + " stale=" + tile.Stale);
            if (!tileLifetime.IsCancellationRequested)
                await sendAsync(new JsonObject { ["type"] = "tile", ["session"] = session, ["id"] = id, ["data"] = tile.Data, ["error"] = tile.Error });
        }
        finally { pending--; requests.Remove(id); }
        return true;
    }
}
