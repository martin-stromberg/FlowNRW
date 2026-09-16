param([string]$Exe, [switch]$Inspect, [string]$ScreenshotDirectory)
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
}
"@
        }
        $handle = [IntPtr]$script:window.Current.NativeWindowHandle
        [NativeWindowCapture]::SetForegroundWindow($handle) | Out-Null
        Start-Sleep -Milliseconds 750
        if ([NativeWindowCapture]::GetForegroundWindow() -ne $handle) { throw 'Own app must be foreground for capture' }
        $rect = $script:window.Current.BoundingRectangle
        $bitmap = New-Object Drawing.Bitmap(([int]$rect.Width - 20), ([int]$rect.Height - 20))
        $graphics = [Drawing.Graphics]::FromImage($bitmap)
        try {
            $graphics.CopyFromScreen(([int]$rect.Left + 10), ([int]$rect.Top + 10), 0, 0, $bitmap.Size)
            $bitmap.Save((Join-Path $ScreenshotDirectory ($name + '.png')), [Drawing.Imaging.ImageFormat]::Png)
        } finally { $graphics.Dispose(); $bitmap.Dispose() }
    }
    Wait 'OriginText' | Out-Null
    Snapshot 'native-search'
    if ($Inspect) {
        $script:window.FindAll([System.Windows.Automation.TreeScope]::Descendants, [System.Windows.Automation.Condition]::TrueCondition) | ForEach-Object { '{0}|{1}|{2}' -f $_.Current.AutomationId, $_.Current.ControlType.ProgrammaticName, $_.Current.Name }
    } else {
        function Back { Click 'NavigationViewBackButton' }
        function Contains([string]$id, [string]$pattern) { Assert ((Name $id) -match $pattern) "$id contains $pattern" }
        function Toggle([string]$id) { (Wait $id).GetCurrentPattern([System.Windows.Automation.TogglePattern]::Pattern).Toggle(); Start-Sleep -Milliseconds 150 }
        function Status([string]$id, [string]$pattern) {
            for ($j = 0; $j -lt 120; $j++) { if ((Name $id) -match $pattern) { return }; Start-Sleep -Milliseconds 100 }
            throw "$id expected $pattern but was $(Name $id)"
        }
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

