namespace FlowNRW.Core.Maps;

/// <summary>Validated limits for the interchangeable HTTPS raster provider.</summary>
public sealed class MapOptions
{
    /// <summary>Creates conservative tile request defaults.</summary>
    public MapOptions() => Timeout = TimeSpan.FromSeconds(10);
    /// <summary>HTTPS template with zoom, column and row placeholders.</summary>
    public string TileUrl { get; init; } = "https://tile.openstreetmap.org/{z}/{x}/{y}.png";
    /// <summary>Visible provider attribution.</summary>
    public string Attribution { get; init; } = "© OpenStreetMap contributors · openstreetmap.org/copyright";
    /// <summary>Smallest requested zoom.</summary>
    public int MinZoom { get; init; } = 1;
    /// <summary>Largest requested zoom.</summary>
    public int MaxZoom { get; init; } = 18;
    /// <summary>Maximum persistent technical cache size.</summary>
    public long CacheMaxBytes { get; init; } = 64 * 1024 * 1024;
    /// <summary>Per-request time limit.</summary>
    public TimeSpan Timeout { get; init; }
    /// <summary>Checks configuration before any requests.</summary>
    public void Validate()
    {
        var sample = TileUrl.Replace("{z}", "1").Replace("{x}", "0").Replace("{y}", "0");
        if (!Uri.TryCreate(sample, UriKind.Absolute, out var uri) || uri.Scheme != "https" || uri.UserInfo.Length > 0
            || uri.Fragment.Length > 0 || sample.Contains('{') || sample.Contains('}')
            || !TileUrl.Contains("{z}") || !TileUrl.Contains("{x}") || !TileUrl.Contains("{y}")
            || string.IsNullOrWhiteSpace(Attribution) || MinZoom < 0 || MaxZoom > 19 || MinZoom > MaxZoom
            || CacheMaxBytes < 2 * 1024 * 1024 || CacheMaxBytes > 256 * 1024 * 1024
            || Timeout <= TimeSpan.Zero || Timeout > TimeSpan.FromSeconds(30)) throw new ArgumentException("Invalid map configuration.");
    }
    /// <summary>Builds only a valid bounded tile address.</summary>
    /// <param name="zoom">Zoom.</param>
    /// <param name="x">Column.</param>
    /// <param name="y">Row.</param>
    /// <returns>Validated HTTPS URI.</returns>
    public Uri TileUri(int zoom, int x, int y)
    {
        Validate();
        if (zoom < MinZoom || zoom > MaxZoom || x < 0 || y < 0 || x >= (1 << zoom) || y >= (1 << zoom))
            throw new ArgumentOutOfRangeException(nameof(zoom));
        return new(TileUrl.Replace("{z}", zoom.ToString(System.Globalization.CultureInfo.InvariantCulture))
            .Replace("{x}", x.ToString(System.Globalization.CultureInfo.InvariantCulture)).Replace("{y}", y.ToString(System.Globalization.CultureInfo.InvariantCulture)));
    }
}
