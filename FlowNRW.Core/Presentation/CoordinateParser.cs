using System.Globalization;
using FlowNRW.Core.Transit;

namespace FlowNRW.Core.Presentation;

/// <summary>Parses explicit latitude and longitude fields without thousands separators.</summary>
public static class CoordinateParser
{
    /// <summary>Validates finite WGS84 values.</summary>
    /// <param name="latitude">Latitude text.</param>
    /// <param name="longitude">Longitude text.</param>
    /// <param name="coordinate">Validated coordinate.</param>
    /// <param name="error">Field-specific validation message.</param>
    /// <returns>Whether both fields are valid.</returns>
    public static bool TryParse(string latitude, string longitude, out GeoCoordinate? coordinate, out string error)
    {
        coordinate = null;
        if (!Number(latitude, out var lat) || lat < -90 || lat > 90) { error = "Breite muss eine Zahl zwischen -90 und 90 sein."; return false; }
        if (!Number(longitude, out var lon) || lon < -180 || lon > 180) { error = "Länge muss eine Zahl zwischen -180 und 180 sein."; return false; }
        coordinate = new GeoCoordinate(lat, lon); error = string.Empty; return true;
    }
    private static bool Number(string text, out double value) => double.TryParse(text.Replace(',', '.'), NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite, CultureInfo.InvariantCulture, out value) && double.IsFinite(value);
}
