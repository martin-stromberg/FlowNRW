param([Parameter(Mandatory = $true)][string]$Exe)
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

try {
    StartApp
    SetText 'HomeScenario' 'favorite-cache-seed'
    Click 'OpenHomeStops'; SetText 'StopQuery' 'Favorite Far'; Click 'FindStops'; Click 'StopMatch0'
    AwaitText 'MonitorStatus' 'manuell aktualisiert'
    SetText 'FavoriteScenario' 'favorite-cache-seed'; Click 'ToggleFavorite'; AwaitText 'FavoriteToggleStatus' 'hinzugefügt|gespeichert'
    Click 'NavigationViewBackButton'; SelectTab 'Abfahrten'; Wait 'FavoriteName0' | Out-Null
    AwaitText 'FavoriteStatus0' 'automatisch aktualisiert'
    Assert ((Name 'FavoriteDeparture0_0') -match 'RE 1 · Stand 1') 'Initial successful favorite response is shown before restart'
    Assert (Test-Path $env:FLOWNRW_UI_TEST_DEPARTURE_CACHE) 'Successful favorite response created an isolated departure cache'

    $env:FLOWNRW_UI_TEST_SCENARIO = 'cache-start-slow-nearby'
    StartApp
    Wait 'FavoriteName0' | Out-Null
    AwaitText 'FavoriteStatus0' 'Letzter Stand wird aktualisiert|werden aktualisiert'
    Assert ((Name 'FavoriteDeparture0_0') -match 'RE 1 · Stand 1') 'Cached future departure is visible before delayed provider completion'
    $started = AwaitCalls 1 3
    Assert ($started.TotalSeconds -lt 3) 'Startup provider refresh begins before the six-second Nearby lookup completes'
    Assert ((DepartureCalls) -eq 1) 'Exactly one startup provider refresh is requested for cached favorite'
    AwaitText 'FavoriteStatus0' 'automatisch aktualisiert'
    Assert ((Name 'FavoriteDeparture0_0') -match 'RE 1 · Live Stand 1') 'Delayed provider response replaces displayed cached departure'
    Write-Output 'PASS native departure-cache startup regression'
}
finally {
    if ($script:app -and !$script:app.HasExited) { Stop-Process -Id $script:app.Id }
    foreach ($key in $keys) { [Environment]::SetEnvironmentVariable($key, $previous[$key]) }
}
