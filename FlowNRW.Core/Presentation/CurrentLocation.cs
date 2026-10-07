using FlowNRW.Core.Transit;

namespace FlowNRW.Core.Presentation;

/// <summary>Outcome of one explicitly requested location lookup.</summary>
/// <param name="Status">Platform outcome.</param>
/// <param name="Coordinate">Validated position, if available.</param>
/// <param name="Timestamp">Time the position was determined.</param>
/// <param name="AccuracyMeters">Optional horizontal accuracy.</param>
/// <param name="IsReducedAccuracy">Whether the platform reports approximate positioning.</param>
/// <returns>Immutable one-shot location outcome.</returns>
public sealed record LocationResult(LocationStatus Status, GeoCoordinate? Coordinate = null,
    DateTimeOffset? Timestamp = null, double? AccuracyMeters = null, bool IsReducedAccuracy = false)
{
    /// <summary>Optional provider or platform detail safe for the UI.</summary>
    public string? Message { get; init; }
    /// <summary>Whether this result contains a usable current position.</summary>
    public bool HasCurrentPosition
    {
        get
        {
            return Status == LocationStatus.Success && Coordinate is not null &&
                (Timestamp is null || Timestamp >= DateTimeOffset.UtcNow.AddMinutes(-2) && Timestamp <= DateTimeOffset.UtcNow.AddMinutes(1));
        }
    }
    /// <summary>Honest optional accuracy information for the current position.</summary>
    public string AccuracyDescription
    {
        get
        {
            return (IsReducedAccuracy ? " Ungefähre Position." : "") +
                (AccuracyMeters is { } accuracy && double.IsFinite(accuracy) && accuracy >= 0
                    ? $" Genauigkeit: ca. {accuracy.ToString("0", System.Globalization.CultureInfo.GetCultureInfo("de-DE"))} m." : "");
        }
    }
    /// <summary>Safe actionable error text independent of external provider details.</summary>
    public string FailureDescription => Status switch
    {
        LocationStatus.Denied => "Standortzugriff nicht erlaubt. Bitte Berechtigung in den Systemeinstellungen prüfen oder manuell suchen.",
        LocationStatus.Disabled => "Standortdienst deaktiviert. Bitte Systemeinstellungen prüfen oder manuell suchen.",
        LocationStatus.Unsupported => "Standort wird auf diesem Gerät nicht unterstützt. Bitte manuell suchen.",
        LocationStatus.Timeout => "Standortabfrage hat zu lange gedauert. Bitte erneut versuchen oder manuell suchen.",
        LocationStatus.Error => "Standort konnte nicht ermittelt werden. Bitte erneut versuchen oder manuell suchen.",
        _ => "Keine aktuelle Position verfügbar. Bitte erneut versuchen oder manuell suchen."
    };
}

/// <summary>Non-exceptional states returned by a location provider.</summary>
public enum LocationStatus
{
    /// <summary>A valid current coordinate was returned.</summary>
    Success,
    /// <summary>Permission was denied.</summary>
    Denied,
    /// <summary>The system location service is disabled.</summary>
    Disabled,
    /// <summary>The platform does not support location.</summary>
    Unsupported,
    /// <summary>The request timed out.</summary>
    Timeout,
    /// <summary>No current position was available.</summary>
    Unavailable,
    /// <summary>An unexpected provider error occurred.</summary>
    Error
}

/// <summary>Provides one current position after an explicit user action.</summary>
public interface ICurrentLocationService
{
    /// <summary>Requests a current position; cancellation is independent per call.</summary>
    /// <param name="cancellationToken">Cancellation for this request.</param>
    /// <returns>Platform result without sensitive diagnostics.</returns>
    Task<LocationResult> GetCurrentAsync(CancellationToken cancellationToken);
}
