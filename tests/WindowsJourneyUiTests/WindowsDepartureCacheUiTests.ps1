param([Parameter(Mandatory = $true)][string]$Exe, [switch]$InventoryOnly, [switch]$ExpiredOnly, [switch]$DetailRetention)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes
Add-Type @"
using System;
using System.Runtime.InteropServices;
public static class DepartureCacheWindow {
 [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr h);
 [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr h, int command);
}
"@

$keys = @('FLOWNRW_UI_TEST_FAVORITES', 'FLOWNRW_UI_TEST_DEPARTURE_CACHE', 'FLOWNRW_UI_TEST_REFRESH_SETTINGS', 'FLOWNRW_UI_TEST_SCENARIO')
$previous = @{}
foreach ($key in $keys) { $previous[$key] = [Environment]::GetEnvironmentVariable($key) }
$directory = Join-Path (Get-Location) ('artifacts/tests/departure-cache/' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Force $directory | Out-Null
$env:FLOWNRW_UI_TEST_FAVORITES = Join-Path $directory 'favorites.json'
$env:FLOWNRW_UI_TEST_DEPARTURE_CACHE = Join-Path $directory 'departure-cache.json'
$env:FLOWNRW_UI_TEST_REFRESH_SETTINGS = Join-Path $directory 'refresh-settings.json'
$env:FLOWNRW_UI_TEST_SCENARIO = 'success'
[IO.File]::WriteAllText($env:FLOWNRW_UI_TEST_REFRESH_SETTINGS, '0')
$script:app = $null
$script:window = $null

function Assert([bool]$value, [string]$message) { if (!$value) { throw $message }; Write-Output ('PASS ' + $message) }
function Find([string]$id) {
    $condition = New-Object System.Windows.Automation.PropertyCondition([System.Windows.Automation.AutomationElement]::AutomationIdProperty, $id)
    $script:window.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $condition)
}
function Wait([string]$id) {
    for ($attempt = 0; $attempt -lt 120; $attempt++) { $element = Find $id; if ($element) { return $element }; Start-Sleep -Milliseconds 100 }
    throw ('Missing native element ' + $id)
}
function Name([string]$id) { (Wait $id).Current.Name }
function Click([string]$id) { (Wait $id).GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern).Invoke(); Start-Sleep -Milliseconds 150 }
function SetText([string]$id, [string]$value) { (Wait $id).GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern).SetValue($value); Start-Sleep -Milliseconds 100 }
function AwaitText([string]$id, [string]$pattern) {
    for ($attempt = 0; $attempt -lt 100; $attempt++) { if ((Name $id) -match $pattern) { return }; Start-Sleep -Milliseconds 100 }
    throw ($id + ' did not reach ' + $pattern + '; actual: ' + (Name $id))
}
function SelectTab([string]$name) {
    $condition = New-Object System.Windows.Automation.PropertyCondition([System.Windows.Automation.AutomationElement]::NameProperty, $name)
    foreach ($item in $script:window.FindAll([System.Windows.Automation.TreeScope]::Descendants, $condition)) {
        $selection = $null
        if ($item.TryGetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern, [ref]$selection)) { $selection.Select(); Start-Sleep -Milliseconds 250; return }
    }
    throw ('Persistent native tab not selectable: ' + $name)
}
function StartApp {
    if ($script:app -and !$script:app.HasExited) { Stop-Process -Id $script:app.Id; $script:app.WaitForExit() }
    $script:app = Start-Process -FilePath $Exe -PassThru -WindowStyle Hidden
    $condition = New-Object System.Windows.Automation.PropertyCondition([System.Windows.Automation.AutomationElement]::ProcessIdProperty, $script:app.Id)
    $script:window = $null
    for ($attempt = 0; $attempt -lt 100 -and !$script:window; $attempt++) {
        Start-Sleep -Milliseconds 200
        $script:window = [System.Windows.Automation.AutomationElement]::RootElement.FindFirst([System.Windows.Automation.TreeScope]::Children, $condition)
    }
    if (!$script:window) { throw 'Native application window unavailable' }
    Wait 'OpenHomeStops' | Out-Null
    $handle = [IntPtr]$script:window.Current.NativeWindowHandle
    [DepartureCacheWindow]::ShowWindow($handle, 9) | Out-Null
    [DepartureCacheWindow]::SetForegroundWindow($handle) | Out-Null
    Start-Sleep -Milliseconds 300
}
function DepartureCalls {
    $text = Name 'FavoriteCalls'
    if ($text -match '(?:^|;)fixture-favorite-far-0=(\d+)(?:;|$)') { return [int]$Matches[1] }
    return 0
}
function AwaitCalls([int]$expected, [int]$maximumSeconds = 12) {
    $clock = [Diagnostics.Stopwatch]::StartNew()
    while ((DepartureCalls) -lt $expected -and $clock.Elapsed.TotalSeconds -lt $maximumSeconds) { Start-Sleep -Milliseconds 100 }
    if ((DepartureCalls) -ge $expected) { return $clock.Elapsed }
    throw ('Expected departure request count ' + $expected + ', actual: ' + (Name 'FavoriteCalls'))
}
function AwaitVisible([string]$id, [int]$maximumSeconds = 12) {
    for ($attempt = 0; $attempt -lt ($maximumSeconds * 10); $attempt++) {
        $element = Find $id
        if ($element -and !$element.Current.IsOffscreen) { return $element }
        Start-Sleep -Milliseconds 100
    }
    throw ('Expected visible native element ' + $id)
}
function AwaitNotBusy([string]$id, [int]$maximumSeconds = 12) {
    for ($attempt = 0; $attempt -lt ($maximumSeconds * 10); $attempt++) {
        $element = Find $id
        if ($element -and $element.Current.IsOffscreen) { return }
        Start-Sleep -Milliseconds 100
    }
    throw ('Expected completed native activity ' + $id)
}
function AssertLines([string]$required, [string]$forbidden, [string]$message) {
    $actual = Name 'FavoriteLines0'
    Assert ($actual -match $required -and $actual -notmatch $forbidden) ($message + '; actual: ' + $actual)
}

try {
    StartApp
    SetText 'HomeScenario' 'favorite-cache-seed'
    Click 'OpenHomeStops'; SetText 'StopQuery' 'Favorite Far'; Click 'FindStops'; Click 'StopMatch0'
    AwaitVisible 'Departure0' | Out-Null
    SetText 'FavoriteScenario' 'favorite-cache-seed'; Click 'ToggleFavorite'; AwaitText 'FavoriteToggleStatus' 'hinzugefügt|gespeichert'
    Click 'NavigationViewBackButton'; SelectTab 'Abfahrten'; Wait 'FavoriteName0' | Out-Null
    AwaitVisible 'FavoriteLines0' | Out-Null
    Click 'ToggleFavorite0'
    AwaitVisible 'FavoriteDeparture0_0' | Out-Null
    Start-Sleep -Milliseconds 250
    $initialDeparture = Name 'FavoriteDeparture0_0'
    Assert ($initialDeparture -match 'RE 1 .*Stand 1') ('Initial successful favorite response is shown before restart; actual: ' + $initialDeparture)
    Click 'ToggleFavorite0'
    Assert ((Name 'FavoriteLines0') -match 'RE 1') 'Collapsing a favorite retains its line overview'
    Assert (Test-Path $env:FLOWNRW_UI_TEST_DEPARTURE_CACHE) 'Successful favorite response created an isolated departure cache'

    if ($InventoryOnly) {
        $env:FLOWNRW_UI_TEST_SCENARIO = 'cache-lines-bc'
        StartApp
        AwaitVisible 'FavoriteBusy0' | Out-Null
        Start-Sleep -Milliseconds 1500
        AssertLines 'B.*C' 'RE 1|S2|107|U11' 'A complete response replaces the persisted line inventory'
        $env:FLOWNRW_UI_TEST_SCENARIO = 'success'
        StartApp
        AwaitVisible 'FavoriteBusy0' | Out-Null
        Start-Sleep -Milliseconds 1500
        AssertLines 'B.*C' 'RE 1|S2|107|U11' 'A warning response does not replace the complete line inventory'
        $env:FLOWNRW_UI_TEST_SCENARIO = 'cache-lines-empty'
        StartApp
        AwaitVisible 'FavoriteBusy0' | Out-Null
        Start-Sleep -Milliseconds 1500
        AssertLines '^$' 'B|C|RE 1|S2|107|U11' 'A complete empty response clears the persisted line inventory'
        Write-Output 'PASS native departure-cache inventory regression'
        return
    }

    if ($ExpiredOnly) {
        foreach ($scenario in @('cache-expired-lines', 'cache-timeless-lines')) {
            $env:FLOWNRW_UI_TEST_SCENARIO = $scenario
            StartApp
            AwaitVisible 'FavoriteBusy0' | Out-Null
            Start-Sleep -Milliseconds 1500
            $line = if ($scenario -eq 'cache-expired-lines') { 'Expired' } else { 'Zeitlos' }
            AssertLines $line 'RE 1|S2|107|U11' ('Expired or timeless response preserves only its line inventory: ' + $line)
            Click 'ToggleFavorite0'
            Assert ($null -eq (Find 'FavoriteDeparture0_0')) ('No unusable departure time is displayed for ' + $line)
            $env:FLOWNRW_UI_TEST_SCENARIO = 'cache-start-error'
            StartApp
            AwaitCalls 1 3 | Out-Null
            AwaitText 'FavoriteStatus0' 'Letzte bekannte|fehlgeschlagen'
            AssertLines $line 'RE 1|S2|107|U11' ('Provider error retains expired or timeless inventory after restart: ' + $line)
        }
        Write-Output 'PASS native departure-cache expired and timeless inventory regression'
        return
    }

    $env:FLOWNRW_UI_TEST_SCENARIO = 'cache-start-slow-nearby'
    StartApp
    Wait 'FavoriteName0' | Out-Null
    AwaitVisible 'FavoriteBusy0' | Out-Null
    Assert ((Name 'FavoriteLines0') -match 'RE 1') 'Collapsed favorite restores its persisted line inventory before provider completion'
    Click 'ToggleFavorite0'
    Assert ((Name 'FavoriteDeparture0_0') -match 'RE 1 .*Stand 1') 'Cached future departure is visible before delayed provider completion'
    $started = AwaitCalls 1 3
    Assert ($started.TotalSeconds -lt 3) 'Startup provider refresh begins before the six-second Nearby lookup completes'
    Assert ((DepartureCalls) -eq 1) 'Exactly one startup provider refresh is requested for cached favorite'
    for ($attempt = 0; $attempt -lt 100 -and (Find 'FavoriteBusy0').Current.IsOffscreen -eq $false; $attempt++) { Start-Sleep -Milliseconds 100 }
    Assert ((Name 'FavoriteDeparture0_0') -match 'RE 1 .*Live Stand 1') 'Delayed provider response replaces displayed cached departure'
    Click 'OpenFavorite0'
    AwaitVisible 'Departure0' | Out-Null
    Assert ((Name 'Departure0') -match 'RE 1 .*Live Stand 1') 'Favorite details immediately adopt the visible cached board while refreshing'
    AwaitVisible 'MonitorBusy' | Out-Null
    $monitorStatus = Find 'MonitorStatus'
    Assert ($null -eq $monitorStatus -or $monitorStatus.Current.Name -notmatch 'werden aktualisiert') 'Detail refresh uses its header symbol instead of a loading text'
    if ($DetailRetention) {
        $env:FLOWNRW_UI_TEST_SCENARIO = 'cache-start-error'
        StartApp
        Click 'OpenFavorite0'
        AwaitVisible 'Departure0' | Out-Null
        Assert ((Name 'Departure0') -match 'RE 1') 'Detail error starts with cached departures'
        AwaitText 'MonitorStatus' 'Letzte bekannte|fehlgeschlagen'
        Assert ((Name 'Departure0') -match 'RE 1') 'Detail error retains cached departures'
        $env:FLOWNRW_UI_TEST_SCENARIO = 'cache-start-cancel'
        StartApp
        Click 'OpenFavorite0'
        AwaitVisible 'MonitorBusy' | Out-Null
        Click 'NavigationViewBackButton'
        SelectTab 'Abfahrten'
        $env:FLOWNRW_UI_TEST_SCENARIO = 'success'
        StartApp
        Assert ((Name 'FavoriteLines0') -match 'RE 1') 'Lifecycle cancellation retains cached inventory after restart'
        Write-Output 'PASS native detail cache handoff regression'
        return
    }
    $env:FLOWNRW_UI_TEST_SCENARIO = 'cache-start-error'
    StartApp
    AwaitVisible 'FavoriteLines0' | Out-Null
    AwaitCalls 1 3 | Out-Null
    AwaitText 'FavoriteStatus0' 'Letzte bekannte|fehlgeschlagen|Abfahrten konnten nicht geladen werden'
    Assert ((Name 'FavoriteLines0') -match 'RE 1') 'Provider failure keeps the persisted line inventory'
    $env:FLOWNRW_UI_TEST_SCENARIO = 'cache-start-cancel'
    StartApp
    AwaitVisible 'FavoriteBusy0' | Out-Null
    $env:FLOWNRW_UI_TEST_SCENARIO = 'success'
    StartApp
    AwaitVisible 'FavoriteLines0' | Out-Null
    Assert ((Name 'FavoriteLines0') -match 'RE 1') 'Cancelled startup refresh keeps the persisted line inventory after restart'
    Write-Output 'PASS native departure-cache startup regression'
}
finally {
    if ($script:app -and !$script:app.HasExited) { Stop-Process -Id $script:app.Id }
    foreach ($key in $keys) { [Environment]::SetEnvironmentVariable($key, $previous[$key]) }
}
