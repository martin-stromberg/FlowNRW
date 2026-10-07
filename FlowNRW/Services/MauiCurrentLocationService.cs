using FlowNRW.Core.Presentation;
using FlowNRW.Core.Transit;
using CoreLocationStatus = FlowNRW.Core.Presentation.LocationStatus;
using CoreLocationResult = FlowNRW.Core.Presentation.LocationResult;

namespace FlowNRW.Services;

/// <summary>MAUI adapter for an explicit, foreground-only position request.</summary>
public sealed class MauiCurrentLocationService : ICurrentLocationService
{
    /// <inheritdoc />
    public async Task<LocationResult> GetCurrentAsync(CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var permission = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            cancellationToken.ThrowIfCancellationRequested();
            if (permission == PermissionStatus.Disabled) return new(CoreLocationStatus.Disabled);
            if (permission == PermissionStatus.Restricted) return new(CoreLocationStatus.Denied);
#if IOS
            if (permission == PermissionStatus.Denied) return new(CoreLocationStatus.Denied);
#endif
            if (permission != PermissionStatus.Granted)
            {
                permission = await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    return Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                });
                cancellationToken.ThrowIfCancellationRequested();
                if (permission == PermissionStatus.Disabled) return new(CoreLocationStatus.Disabled);
                if (permission != PermissionStatus.Granted)
                    return new CoreLocationResult(CoreLocationStatus.Denied) { Message = "Standortberechtigung nicht erteilt." };
            }
            if (!Geolocation.IsEnabled)
                return new CoreLocationResult(CoreLocationStatus.Disabled) { Message = "Standortdienst ist deaktiviert." };
            var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(15));
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(15));
            Location? location;
            try
            {
                location = await Geolocation.Default.GetLocationAsync(request, timeout.Token).WaitAsync(timeout.Token);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                return new CoreLocationResult(CoreLocationStatus.Timeout);
            }
            cancellationToken.ThrowIfCancellationRequested();
            if (location is null) return new CoreLocationResult(CoreLocationStatus.Unavailable) { Message = "Keine aktuelle Position verfügbar." };
            var coordinate = new GeoCoordinate(location.Latitude, location.Longitude);
            var timestamp = location.Timestamp.ToUniversalTime();
            if (timestamp > DateTimeOffset.UtcNow.AddMinutes(1) || timestamp < DateTimeOffset.UtcNow.AddMinutes(-2))
                return new CoreLocationResult(CoreLocationStatus.Unavailable) { Message = "Zeitstempel der Position ist ungültig." };
            var accuracy = location.Accuracy is { } value && double.IsFinite(value) && value >= 0 ? location.Accuracy : null;
            return new CoreLocationResult(CoreLocationStatus.Success, coordinate, timestamp, accuracy, location.ReducedAccuracy);
        }
        catch (OperationCanceledException) { throw; }
        catch (FeatureNotSupportedException) { return new CoreLocationResult(CoreLocationStatus.Unsupported) { Message = "Standort wird auf diesem Gerät nicht unterstützt." }; }
        catch (FeatureNotEnabledException) { return new CoreLocationResult(CoreLocationStatus.Disabled); }
        catch (PermissionException) { return new CoreLocationResult(CoreLocationStatus.Denied) { Message = "Standortberechtigung nicht erteilt." }; }
        catch (Exception) { return new CoreLocationResult(CoreLocationStatus.Error) { Message = "Standort konnte nicht ermittelt werden." }; }
    }
}
