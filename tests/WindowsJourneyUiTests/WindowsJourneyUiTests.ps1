param([string]$Exe, [switch]$Inspect, [string]$ScreenshotDirectory, [switch]$Monitors, [switch]$LiveMonitors, [switch]$Maps, [switch]$LiveMaps)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes
$app = Start-Process -FilePath $Exe -PassThru -WindowStyle Hidden
try {
    $condition = New-Object System.Windows.Automation.PropertyCondition([System.Windows.Automation.AutomationElement]::ProcessIdProperty, $app.Id)
    $script:window = $null
    for ($i = 0; $i -lt 100 -and !$script:window; $i++) {
        Start-Sleep -Milliseconds 200
        $script:window = [System.Windows.Automation.AutomationElement]::RootElement.FindFirst([System.Windows.Automation.TreeScope]::Children, $condition)
    }
    if (!$script:window) { throw 'Native window not found' }
    function Find([string]$id) {
        $c = New-Object System.Windows.Automation.PropertyCondition([System.Windows.Automation.AutomationElement]::AutomationIdProperty, $id)
        return $script:window.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $c)
    }
    function Wait([string]$id) {
        for ($i = 0; $i -lt 100; $i++) { $e = Find $id; if ($e) { return $e }; Start-Sleep -Milliseconds 100 }
        throw "Missing $id"
    }
    function Click([string]$id) { (Wait $id).GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern).Invoke(); Start-Sleep -Milliseconds 150 }
    function SetText([string]$id, [string]$value) { (Wait $id).GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern).SetValue($value); Start-Sleep -Milliseconds 100 }
    function Name([string]$id) { return (Wait $id).Current.Name }
    function Assert([bool]$value, [string]$message) { if (!$value) { throw $message }; Write-Output "PASS $message" }
    function Back { Click 'NavigationViewBackButton' }
    function Contains([string]$id, [string]$pattern) { Assert ((Name $id) -match $pattern) "$id contains $pattern" }
    function Status([string]$id, [string]$pattern) {
        for ($j = 0; $j -lt 120; $j++) { if ((Name $id) -match $pattern) { return }; Start-Sleep -Milliseconds 100 }
        throw "$id expected $pattern but was $(Name $id)"
    }
    function SelectEndpoint([string]$prefix, [string]$value) { SetText ($prefix + 'Text') $value; Click ($prefix + 'Search'); Click ($prefix + 'Match0') }
    function Snapshot([string]$name) {
        if (!$ScreenshotDirectory) { return }
        Add-Type -AssemblyName System.Drawing
        if (!('NativeWindowCapture' -as [type])) {
            Add-Type @"
using System;
using System.Runtime.InteropServices;
public static class NativeWindowCapture {
    [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr window);
    [DllImport("user32.dll")] public static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")] public static extern bool PrintWindow(IntPtr window, IntPtr dc, uint flags);
}
"@
        }
        $handle = [IntPtr]$script:window.Current.NativeWindowHandle
        [NativeWindowCapture]::SetForegroundWindow($handle) | Out-Null
        Start-Sleep -Milliseconds 750
        $foreground = [NativeWindowCapture]::GetForegroundWindow() -eq $handle
        $rect = $script:window.Current.BoundingRectangle
        $bitmap = New-Object Drawing.Bitmap(([int]$rect.Width - 20), ([int]$rect.Height - 20))
        $graphics = [Drawing.Graphics]::FromImage($bitmap)
        try {
                        if ($foreground) {
                $graphics.CopyFromScreen(([int]$rect.Left + 10), ([int]$rect.Top + 10), 0, 0, $bitmap.Size)
            } else {
                $dc = $graphics.GetHdc()
                try { if (![NativeWindowCapture]::PrintWindow($handle, $dc, 2)) { throw 'Own window capture unavailable' } }
                finally { $graphics.ReleaseHdc($dc) }
            }
            $bitmap.Save((Join-Path $ScreenshotDirectory ($name + '.png')), [Drawing.Imaging.ImageFormat]::Png)
        } finally { $graphics.Dispose(); $bitmap.Dispose() }
    }
    Wait 'OriginText' | Out-Null
    Snapshot 'native-search'
    if ($LiveMaps) {
        Click 'OpenStopSearch'; SetText 'StopQuery' 'Gelsenkirchen Hbf'; Click 'FindStops'
        for ($attempt=0; $attempt -lt 120 -and !(Find 'StopMatch0'); $attempt++) { Start-Sleep -Milliseconds 500 }
        Write-Output ('LIVE stop: '+(Name 'StopMatch0')); Click 'ShowStopMap'
        for ($attempt=0; $attempt -lt 120 -and (Name 'MapStatus') -match 'wird geladen'; $attempt++) { Start-Sleep -Milliseconds 500 }
        Write-Output ('LIVE map: '+(Name 'MapStatus')); Contains 'MapStatus' 'Basiskarte geladen\.'
        Write-Output ('LIVE attribution: '+(Name 'MapAttribution')); Snapshot 'native-live-map'
        Click 'ShowMapList'; Click 'MapStation0'
        for ($attempt=0; $attempt -lt 120 -and (Name 'MonitorStatus') -match 'werden'; $attempt++) { Start-Sleep -Milliseconds 500 }
        Contains 'MonitorStatus' 'manuell aktualisiert'; Write-Output ('LIVE monitor: '+(Name 'MonitorStop')+'; '+(Name 'MonitorMetadata'))
        Back; Back; Back; Wait 'OriginText' | Out-Null
        Write-Output 'PASS live stop map, basemap, attribution, monitor selection and back navigation'
    } elseif ($Maps) {
        Add-Type @"
using System;
using System.Runtime.InteropServices;
public static class MapPointer {
 [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr h);
 [DllImport("user32.dll")] public static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")] public static extern bool PrintWindow(IntPtr window, IntPtr dc, uint flags);
 [DllImport("user32.dll")] public static extern bool SetCursorPos(int x,int y);
 [DllImport("user32.dll")] public static extern void mouse_event(uint flags,uint dx,uint dy,uint data,UIntPtr info);
 public static void Move(int dx,int dy) { mouse_event(1,unchecked((uint)dx),unchecked((uint)dy),0,UIntPtr.Zero); }
}
"@
        function MapClick([double]$x,[double]$y) {
            $handle=[IntPtr]$script:window.Current.NativeWindowHandle
            [MapPointer]::SetForegroundWindow($handle) | Out-Null
            Start-Sleep -Milliseconds 200
            if ([MapPointer]::GetForegroundWindow() -ne $handle) { $script:window.SetFocus(); Start-Sleep -Milliseconds 300 }
            if ([MapPointer]::GetForegroundWindow() -ne $handle) { throw 'Own map window must be foreground' }
            $rect=(Wait 'MapCanvas').Current.BoundingRectangle
            [MapPointer]::SetCursorPos([int]($rect.Left+$rect.Width*$x),[int]($rect.Top+$rect.Height*$y)) | Out-Null
            [MapPointer]::mouse_event(2,0,0,0,[UIntPtr]::Zero); [MapPointer]::mouse_event(4,0,0,0,[UIntPtr]::Zero)
            Start-Sleep -Milliseconds 500
        }
        function MapPan {
            MapClick 0.7 0.6
            $rect=(Wait 'MapCanvas').Current.BoundingRectangle
            [MapPointer]::mouse_event(2,0,0,0,[UIntPtr]::Zero)
            for ($step=1; $step -le 10; $step++) {
                [MapPointer]::Move(-20,0)
                Start-Sleep -Milliseconds 30
            }
            [MapPointer]::mouse_event(4,0,0,0,[UIntPtr]::Zero)
            Start-Sleep -Milliseconds 600
        }
        function KeyboardActivate([string]$id) {
            Add-Type -AssemblyName System.Windows.Forms
            (Wait $id).SetFocus()
            if ([MapPointer]::GetForegroundWindow() -ne [IntPtr]$script:window.Current.NativeWindowHandle) { throw 'Own map window must be foreground for keyboard input' }
            [System.Windows.Forms.SendKeys]::SendWait('{ENTER}')
            Start-Sleep -Milliseconds 200
        }
        Click 'OpenStopSearch'; SetText 'StopQuery' 'Map Essen'; Click 'FindStops'; Wait 'StopMatch1' | Out-Null
        Click 'ShowStopMap'; Status 'MapStatus' 'Basiskarte geladen\.'
        Contains 'MapDataStatus' '2 Haltestellen.*2 Kartenpositionen'
        Start-Sleep -Milliseconds 500
        Snapshot 'native-map'
        MapClick 0.5 0.285
        Status 'MonitorStatus' 'manuell aktualisiert'; Contains 'MonitorMetadata' 'fixture-1'
        Back; Status 'MapStatus' 'Basiskarte geladen\.'
        Click 'ShowMapList'; KeyboardActivate 'MapStation0'; Status 'MonitorStatus' 'manuell aktualisiert'; Contains 'MonitorMetadata' 'fixture-0'
        Back; Back; Wait 'StopQuery' | Out-Null
                SetText 'StopQuery' 'map-missing'; Click 'FindStops'; Wait 'StopMatch0' | Out-Null; Click 'ShowStopMap'
        Contains 'MapDataStatus' '0 Kartenpositionen'; Click 'ShowMapList'; Contains 'MapStation0' 'Keine Kartenposition'
        Click 'MapStation0'; Status 'MonitorStatus' 'manuell aktualisiert'; Back; Back
        SetText 'StopQuery' 'map-offline'; Click 'FindStops'; Wait 'StopMatch0' | Out-Null; Click 'ShowStopMap'
        Status 'MapStatus' 'fehlt oder ist veraltet'; Click 'ShowMapList'; Click 'MapStation0'; Status 'MonitorStatus' 'manuell aktualisiert'; Back
        Click 'ShowMapCanvas'; Status 'MapStatus' 'Basiskarte geladen\.'
        MapClick 0.018 0.08; Status 'MapStatus' 'Basiskarte geladen\.'; $beforePan=Name 'MapViewport'; MapPan; Assert ((Name 'MapViewport') -ne $beforePan) 'Pan changes actual map viewport'; Status 'MapStatus' 'Basiskarte geladen\.'; Snapshot 'native-map-panned'
        Click 'ResetMap'; Status 'MapStatus' 'Basiskarte geladen\.'
        Snapshot 'native-map'
        if ($ScreenshotDirectory) {
            $bounds=$script:window.Current.BoundingRectangle
            $transform=$script:window.GetCurrentPattern([System.Windows.Automation.TransformPattern]::Pattern)
            $transform.Resize(430,900); Click 'ShowMapList'; Snapshot 'native-map-list-narrow'
            $transform.Resize($bounds.Width,$bounds.Height)
        }
        Back
        SetText 'StopQuery' '<img src=x onerror=alert(1)>'; Click 'FindStops'; Wait 'StopMatch0' | Out-Null; Click 'ShowStopMap'
        Status 'MapStatus' 'Basiskarte geladen\.'; Click 'ShowMapList'; Contains 'MapStation0' 'onerror'
        Click 'MapStation0'; Status 'MonitorStatus' 'manuell aktualisiert'; Contains 'MonitorStop' 'onerror'; Back; Back
        SetText 'StopQuery' 'map-slow'; Click 'FindStops'; Wait 'StopMatch0' | Out-Null; Click 'ShowStopMap'
        Back; SetText 'StopQuery' 'Newest map'; Click 'FindStops'; Wait 'StopMatch0' | Out-Null; Click 'ShowStopMap'
        Status 'MapStatus' 'Basiskarte geladen\.'; Start-Sleep -Seconds 6; Contains 'MapMetadata' 'Newest map'; Back; Back
        SelectEndpoint 'Origin' 'Essen'; SelectEndpoint 'Destination' 'Berlin'; Click 'SearchJourneys'; Click 'Journey0'
        Click 'ShowJourneyMap'; Status 'MapStatus' 'Basiskarte geladen\.'; Contains 'MapDataStatus' 'Teilweiser Verlauf'; Contains 'MapSegment0' 'RE 1.*3 gelieferte Punkte'; Contains 'MapSegment1' 'Fußweg.*2 gelieferte Punkte'
        Snapshot 'native-journey-map'
        Back; Wait 'JourneyDetailSection1' | Out-Null; Back; Click 'Journey1'; Click 'ShowJourneyMap'
        Contains 'MapDataStatus' 'Keine darstellbare Geometrie'; Assert ($null -eq (Find 'MapSegment0')) 'Previous journey geometry removed'
        Back; Back; Back; Wait 'OriginText' | Out-Null
        Write-Output 'PASS native map marker/list, missing positions, offline recovery, zoom, late tiles, journey geometry and back navigation'
    } elseif ($LiveMonitors) {
        Click 'OpenStopSearch'
        SetText 'StopQuery' 'Gelsenkirchen Hbf'; Click 'FindStops'
        for ($attempt = 0; $attempt -lt 120 -and !(Find 'StopMatch0'); $attempt++) { Start-Sleep -Milliseconds 500 }
        Write-Output ('LIVE selected: ' + (Name 'StopMatch0'))
        Click 'StopMatch0'
        for ($attempt = 0; $attempt -lt 120 -and (Name 'MonitorStatus') -match 'werden'; $attempt++) { Start-Sleep -Milliseconds 500 }
        Contains 'MonitorStatus' 'manuell aktualisiert'
        Write-Output ('LIVE status: ' + (Name 'MonitorStatus'))
        Write-Output ('LIVE metadata: ' + (Name 'MonitorMetadata'))
        Write-Output ('LIVE departure: ' + (Name 'Departure0'))
        Click 'RefreshDepartures'
        for ($attempt = 0; $attempt -lt 120 -and (Name 'MonitorStatus') -match 'werden'; $attempt++) { Start-Sleep -Milliseconds 500 }
        Contains 'MonitorStatus' 'manuell aktualisiert'
        Write-Output ('LIVE refreshed: ' + (Name 'MonitorMetadata'))
        Back; Assert (((Wait 'StopQuery').GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern).Current.Value) -eq 'Gelsenkirchen Hbf') 'Live back preserves query'; Back; Wait 'OriginText' | Out-Null
        Write-Output 'PASS live monitor lookup, departure board, manual refresh and back navigation'
    } elseif ($Monitors) {
        Click 'OpenStopSearch'
        Wait 'StopQuery' | Out-Null
        Click 'FindStops'; Contains 'StopSearchStatus' 'Suchtext'
        foreach ($query in @('Adresse Essen', 'empty', 'error')) {
            SetText 'StopQuery' $query; Click 'FindStops'; Status 'StopSearchStatus' 'Keine Haltestellen|fehlgeschlagen'
            Assert ($null -eq (Find 'StopMatch0')) "Only stops offered: $query"
        }
        SetText 'StopQuery' 'monitor-sequence'; Click 'FindStops'; Click 'StopMatch1'
        Contains 'MonitorStop' 'monitor-sequence'; Status 'MonitorStatus' 'manuell aktualisiert'
        Contains 'MonitorMetadata' 'fixture-1.*Datenalter:.*Fallback.*Veralteter Cache'
        Contains 'Departure0' 'RE 1.*Stand 1'; Contains 'Departure0' '\+3 Min\.'; Contains 'Departure0' 'Gleis-/Steigwechsel'
        Contains 'Departure1' 'Pünktlich gemeldet'; Contains 'Departure2' 'keine Echtzeitdaten'; Contains 'Departure3' 'FÄLLT AUS'
        Snapshot 'native-departures'
        if ($ScreenshotDirectory) {
            $bounds = $script:window.Current.BoundingRectangle
            $transform = $script:window.GetCurrentPattern([System.Windows.Automation.TransformPattern]::Pattern)
            $transform.Resize(430, 900); Snapshot 'native-departures-narrow'; $transform.Resize($bounds.Width, $bounds.Height)
        }
        Click 'RefreshDepartures'; Status 'MonitorStatus' 'manuell aktualisiert'; Contains 'Departure0' 'Stand 2'
        $retained = Name 'Departure0'; $retainedMetadata = Name 'MonitorMetadata'
        Click 'RefreshDepartures'; Status 'MonitorStatus' 'Letzte bekannte Daten'
        Assert ((Name 'Departure0') -eq $retained) 'Failed refresh retains departures'
        Assert ((Name 'MonitorMetadata') -eq $retainedMetadata) 'Failed refresh retains source and timestamp'
        Click 'RefreshDepartures'; Status 'MonitorStatus' 'Keine nächsten'
        Assert ($null -eq (Find 'Departure0')) 'Successful empty refresh clears obsolete departures'
        Click 'RefreshDepartures'; Status 'MonitorStatus' 'manuell aktualisiert'; Contains 'Departure0' 'Stand 5'
        Back
        Assert (((Wait 'StopQuery').GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern).Current.Value) -eq 'monitor-sequence') 'Back preserves stop search'
        SetText 'StopQuery' 'monitor-error'; Click 'FindStops'; Click 'StopMatch0'; Status 'MonitorStatus' 'nicht geladen'
        Assert ($null -eq (Find 'Departure0')) 'New stop does not retain previous stop departures'
        Back
        SetText 'StopQuery' 'monitor-slow'; Click 'FindStops'; Click 'StopMatch0'; Contains 'MonitorStatus' 'aktualisiert'
        Back
        SetText 'StopQuery' 'New Stop'; Click 'FindStops'; Click 'StopMatch0'; Status 'MonitorStatus' 'manuell aktualisiert'
        Start-Sleep -Seconds 6
        Contains 'MonitorStop' 'New Stop'; Contains 'MonitorMetadata' 'New Stop'; Contains 'Departure0' 'Stand 1'
        Back; Back; Wait 'OriginText' | Out-Null
        Write-Output 'PASS all monitor native UI scenarios'
    } elseif ($Inspect) {
        $script:window.FindAll([System.Windows.Automation.TreeScope]::Descendants, [System.Windows.Automation.Condition]::TrueCondition) | ForEach-Object { '{0}|{1}|{2}' -f $_.Current.AutomationId, $_.Current.ControlType.ProgrammaticName, $_.Current.Name }
    } else {
        function Toggle([string]$id) { (Wait $id).GetCurrentPattern([System.Windows.Automation.TogglePattern]::Pattern).Toggle(); Start-Sleep -Milliseconds 150 }
        function RoundTrip {
            Click 'SearchJourneys'; Wait 'Journey0' | Out-Null; Back; Wait 'OriginText' | Out-Null
        }
        Assert (!(Wait 'SearchJourneys').Current.IsEnabled) 'Start: routing disabled without endpoints'
        Click 'OriginSearch'; Contains 'OriginStatus' 'Suchtext'
        SetText 'OriginText' 'unselected'; Assert (!(Wait 'SearchJourneys').Current.IsEnabled) 'Validation: unselected text cannot route'
        SelectEndpoint 'Origin' 'Adresse Essen'
        Assert ($null -ne (Find 'OriginMatch1')) 'AddressEndpoints: ambiguous candidates rendered'
        Click 'OriginMatch1'; Contains 'OriginSelection' 'Treffer 1'
        SelectEndpoint 'Destination' 'Adresse Berlin'
        Contains 'DestinationSelection' 'Adresse Berlin'
        Contains 'OriginMetadata' 'Fallback.*Veralteter Cache.*Anbieterwarnung'
        Contains 'OriginMetadata' 'Quelle:.*Datenalter:'
        $selectedOrigin = Name 'OriginSelection'; $selectedDestination = Name 'DestinationSelection'
        Click 'SearchJourneys'; Contains 'RoutingStatus' 'geladen'
        $first = Name 'Journey0'; $second = Name 'Journey1'
        Assert ($first -match '16.09.2026 23:55' -and $second -match '17.09.2026 00:55') 'ResultsAndDetails: ordered results and midnight offset'
        Contains 'Journey0' 'UTC\+02:00'; Contains 'Journey0' '1 Umstiege.*RE 1.*Fixture Bahn.*Fußweg.*Bus 10'
        Contains 'ResultsMetadata' 'Quelle:.*Datenalter:.*Fallback.*Veralteter Cache.*Anbieterwarnung'
        Snapshot 'native-results'
        if ($ScreenshotDirectory) {
            $bounds = $script:window.Current.BoundingRectangle
            $transform = $script:window.GetCurrentPattern([System.Windows.Automation.TransformPattern]::Pattern)
            $transform.Resize(430, 900)
            Snapshot 'native-results-narrow'
            $transform.Resize($bounds.Width, $bounds.Height)
        }
        Click 'Journey0'; Contains 'JourneyDetailSection1' 'Ausfall'; Contains 'JourneyDetailSection1' 'keine Echtzeitdaten'
        Contains 'JourneyDetailSection1' '23:58'; Contains 'JourneyDetailSection2' '300 m'; Contains 'JourneyDetailSection4' 'Umstieg.*10 Min'
        Snapshot 'native-details'
        Back; Assert ((Name 'Journey0') -eq $first) 'BackNavigation: result preserved'
        Back; Assert ((Name 'OriginSelection') -eq $selectedOrigin -and (Name 'DestinationSelection') -eq $selectedDestination) 'BackNavigation: endpoints preserved'
        SetText 'OriginText' 'Essen Hbf'; Assert (!(Wait 'SearchJourneys').Current.IsEnabled) 'BackNavigation: input invalidates old route'
        Assert ((Name 'RoutingMetadata') -eq '') 'BackNavigation: old route metadata cleared'
        SelectEndpoint 'Origin' 'Essen Hbf'; SelectEndpoint 'Destination' 'Berlin Hbf'
        Contains 'OriginSelection' 'fixture-0.*de:05113'; Contains 'DestinationSelection' 'fixture-0.*de:05113'
        RoundTrip; Write-Output 'PASS StopEndpoints'
        Toggle 'OriginCoordinateMode'; Click 'OriginSearch'; Contains 'OriginStatus' 'Breite|Länge'
        SetText 'OriginLatitude' '91'; SetText 'OriginLongitude' '7'; Click 'OriginSearch'; Contains 'OriginStatus' 'Breite'
        Assert (!(Wait 'SearchJourneys').Current.IsEnabled) 'Validation: invalid coordinate cannot route'
        SetText 'OriginLatitude' '51,4556'; SetText 'OriginLongitude' '7.0116'; Click 'OriginSearch'
        Toggle 'DestinationCoordinateMode'; SetText 'DestinationLatitude' '52.52'; SetText 'DestinationLongitude' '13,405'; Click 'DestinationSearch'
        Contains 'OriginSelection' '51.4556'; Contains 'DestinationSelection' '13.405'
        Click 'SearchJourneys'; Wait 'Journey0' | Out-Null; Back
        Toggle 'OriginCoordinateMode'; Toggle 'DestinationCoordinateMode'; Write-Output 'PASS CoordinateEndpoints and Validation recovery'
        SetText 'OriginText' 'slow old'; Click 'OriginSearch'; Contains 'OriginStatus' 'geladen'
        SetText 'DestinationText' 'Parallel Ziel'; Click 'DestinationSearch'
        SelectEndpoint 'Origin' 'Newest Start'; Click 'DestinationMatch0'
        Start-Sleep -Seconds 6
        Contains 'OriginSelection' 'Newest Start'; Contains 'OriginMatch0' 'Newest Start'; Contains 'DestinationSelection' 'Parallel Ziel'
        Write-Output 'PASS LatestInputWins and independent endpoint requests'
        foreach ($query in @('empty', 'error')) {
            SetText 'OriginText' $query; Click 'OriginSearch'; Status 'OriginStatus' 'Keine Treffer|fehlgeschlagen'
            Assert ($null -eq (Find 'OriginMatch0')) "EmptyAndErrorRecovery: $query has no candidates"
        }
        foreach ($query in @('route-empty', 'route-error')) {
            SelectEndpoint 'Origin' $query; Click 'SearchJourneys'; Status 'RoutingStatus' 'Keine Verbindungen|fehlgeschlagen'
            Assert ((Wait 'SearchJourneys').Current.IsEnabled) "EmptyAndErrorRecovery: $query allows retry"
        }
        SelectEndpoint 'Origin' 'Recovered'; RoundTrip; Write-Output 'PASS EmptyAndErrorRecovery'
        SelectEndpoint 'Origin' 'route-slow old'; Click 'SearchJourneys'; Contains 'RoutingStatus' 'geladen'
        SelectEndpoint 'Origin' 'New Route'; Click 'SearchJourneys'
        Contains 'ResultsMetadata' 'New Route'; $newResult = Name 'Journey0'; Click 'Journey0'
        Start-Sleep -Seconds 6
        Contains 'JourneyDetailSection1' 'New Route'; Assert ($null -eq (Find 'ResultsStatus')) 'LatestRouteWins: old response did not navigate away from detail'
        Back; Assert ((Name 'Journey0') -eq $newResult) 'LatestRouteWins: newest result unchanged'; Contains 'ResultsMetadata' 'New Route'
        Write-Output 'PASS all fixture native UI scenarios'
    }
} finally { if (!$app.HasExited) { Stop-Process -Id $app.Id } }

