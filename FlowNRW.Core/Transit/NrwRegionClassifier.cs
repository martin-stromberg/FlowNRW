using System.Text.Json;

namespace FlowNRW.Core.Transit;

/// <summary>Point-in-polygon classification using the official NRW DVG2 boundary.</summary>
public sealed class NrwRegionClassifier : INrwRegionClassifier
{
    private static readonly double[][][][] Polygons = Load();

    /// <summary>Embedded boundary dataset version.</summary>
    public string BoundaryVersion => "GeoBasis-NRW-DVG2-2023-06-12";

    /// <inheritdoc />
    public bool IsInNrw(GeoCoordinate? coordinate)
    {
        if (coordinate is null) return false;
        foreach (var polygon in Polygons)
        {
            if (!InsideRing(polygon[0], coordinate)) continue;
            if (!polygon.Skip(1).Any(hole => InsideRing(hole, coordinate))) return true;
        }
        return false;
    }

    private static bool InsideRing(double[][] ring, GeoCoordinate point)
    {
        bool inside = false;
        double x = point.Longitude, y = point.Latitude;
        for (int i = 0, j = ring.Length - 1; i < ring.Length; j = i++)
        {
            var a = ring[j];
            var b = ring[i];
            double cross = (x - a[0]) * (b[1] - a[1]) - (y - a[1]) * (b[0] - a[0]);
            if (Math.Abs(cross) < 1e-12 && x >= Math.Min(a[0], b[0]) && x <= Math.Max(a[0], b[0]) &&
                y >= Math.Min(a[1], b[1]) && y <= Math.Max(a[1], b[1])) return true;
            if ((a[1] > y) != (b[1] > y) && x < (b[0] - a[0]) * (y - a[1]) / (b[1] - a[1]) + a[0])
                inside = !inside;
        }
        return inside;
    }

    private static double[][][][] Load()
    {
        using var stream = typeof(NrwRegionClassifier).Assembly.GetManifestResourceStream("FlowNRW.Core.Transit.Resources.nrw-dvg2-2023.geojson")
            ?? throw new InvalidOperationException("Official NRW boundary resource missing.");
        using var document = JsonDocument.Parse(stream);
        return document.RootElement.GetProperty("features")[0].GetProperty("geometry").GetProperty("coordinates")
            .Deserialize<double[][][][]>() ?? throw new InvalidOperationException("Invalid NRW boundary resource.");
    }
}
