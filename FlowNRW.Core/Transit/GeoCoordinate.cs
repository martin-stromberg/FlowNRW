namespace FlowNRW.Core.Transit;

/// <summary>A validated finite WGS84 position.</summary>
public sealed record GeoCoordinate
{
    /// <summary>Constructs a WGS84 position.</summary>
    /// <param name="latitude">Latitude in degrees.</param>
    /// <param name="longitude">Longitude in degrees.</param>
    public GeoCoordinate(double latitude, double longitude)
    {
        if (!double.IsFinite(latitude) || latitude is < -90 or > 90 ||
            !double.IsFinite(longitude) || longitude is < -180 or > 180)
            throw new ArgumentOutOfRangeException(nameof(latitude), "Invalid WGS84 coordinate.");
        Latitude = latitude;
        Longitude = longitude;
    }

    /// <summary>Latitude in degrees.</summary>
    public double Latitude { get; }
    /// <summary>Longitude in degrees.</summary>
    public double Longitude { get; }
}
