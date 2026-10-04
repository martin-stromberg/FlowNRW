param([Parameter(Mandatory = $true)][string]$Exe, [string]$ScreenshotDirectory, [switch]$InventoryOnly, [switch]$ExpiredOnly, [switch]$DetailRetention)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes
Add-Type @"
using System;
using System.Runtime.InteropServices;
public static class DepartureCacheWindow {
 [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr h);
 [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr h, int command);
 [DllImport("user32.dll")] public static extern IntPtr GetForegroundWindow();
 [DllImport("user32.dll")] public static extern uint GetDpiForWindow(IntPtr h);
 [DllImport("user32.dll")] public static extern bool PrintWindow(IntPtr h, IntPtr dc, uint flags);
}
"@

if ($ScreenshotDirectory) {
    New-Item -ItemType Directory -Force $ScreenshotDirectory | Out-Null
    $ScreenshotDirectory = (Resolve-Path $ScreenshotDirectory).Path
}
$keys = @('FLOWNRW_UI_TEST_FAVORITES', 'FLOWNRW_UI_TEST_DEPARTURE_CACHE', 'FLOWNRW_UI_TEST_REFRESH_SETTINGS', 'FLOWNRW_UI_TEST_SCENARIO', 'FLOWNRW_UI_TEST_HIDE_CONTROLS')
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
$script:manifest = $null
function Snapshot([string]$name, [string]$scenario) {
    if (!$ScreenshotDirectory) { return }
    Add-Type -AssemblyName System.Drawing
    $handle = [IntPtr]$script:window.Current.NativeWindowHandle
    [DepartureCacheWindow]::SetForegroundWindow($handle) | Out-Null
    Start-Sleep -Milliseconds 400
    $rect = $script:window.Current.BoundingRectangle
    $bitmap = New-Object Drawing.Bitmap(([int]$rect.Width), ([int]$rect.Height))
    $graphics = [Drawing.Graphics]::FromImage($bitmap)
    try {
        $dc = $graphics.GetHdc()
        try { if (![DepartureCacheWindow]::PrintWindow($handle, $dc, 2)) { throw 'Own window capture unavailable' } }
        finally { $graphics.ReleaseHdc($dc) }
        $bitmap.Save((Join-Path $ScreenshotDirectory ($name + '.png')), [Drawing.Imaging.ImageFormat]::Png)
    } finally { $graphics.Dispose(); $bitmap.Dispose() }
    if ($null -eq $script:manifest) {
        $commit = (git rev-parse HEAD).Trim()
        $statusLines = @(git status --porcelain)
        $tracked = @($statusLines | Where-Object { $_ -notmatch '^\?\?' })
        $untracked = @($statusLines | Where-Object { $_ -match '^\?\?' })
        $workingTree = if ($tracked.Count -gt 0) { 'tracked files modified' } elseif ($untracked.Count -gt 0) { 'tracked files clean; untracked files present' } else { 'clean' }
        $buildPath = Join-Path (Split-Path $Exe) 'FlowNRW.dll'
        $sha256 = [Security.Cryptography.SHA256]::Create()
        try { $buildHash = ([BitConverter]::ToString($sha256.ComputeHash([IO.File]::ReadAllBytes($buildPath)))).Replace('-', '') }
        finally { $sha256.Dispose() }
        $script:manifest = @{ commit = $commit; workingTree = $workingTree; buildHash = $buildHash; dpi = [DepartureCacheWindow]::GetDpiForWindow($handle) }
    }
    $bounds = $script:window.Current.BoundingRectangle
    $theme = if ($env:FLOWNRW_UI_TEST_THEME) { $env:FLOWNRW_UI_TEST_THEME } else { 'system' }
    $textScale = if ($env:FLOWNRW_UI_TEST_TEXT_SCALE) { [int]$env:FLOWNRW_UI_TEST_TEXT_SCALE } else { 100 }
    [pscustomobject]@{ image = $name + '.png'; commit = $script:manifest.commit; workingTree = $script:manifest.workingTree; buildSha256 = $script:manifest.buildHash; platform = 'Windows native MAUI'; theme = $theme; logicalWidth = [int][Math]::Round($bounds.Width * 96 / $script:manifest.dpi); logicalHeight = [int][Math]::Round($bounds.Height * 96 / $script:manifest.dpi); physicalWidth = [int]$bounds.Width; physicalHeight = [int]$bounds.Height; dpi = $script:manifest.dpi; textScale = $textScale; scaleMethod = 'UiTest app text scaling'; scenario = $scenario; reference = 'abfahrtsmonitor_live'; fixtureControls = 'hidden'; timestamp = [DateTimeOffset]::Now.ToString('O') } | ConvertTo-Json -Compress | Add-Content (Join-Path $ScreenshotDirectory 'matrix.jsonl')
    Write-Output ('CAPTURE ' + $name)
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
    if ($ScreenshotDirectory) { $env:FLOWNRW_UI_TEST_HIDE_CONTROLS = '1' }
    StartApp
    if ($ScreenshotDirectory) { $env:FLOWNRW_UI_TEST_HIDE_CONTROLS = $previous['FLOWNRW_UI_TEST_HIDE_CONTROLS'] }
    Wait 'FavoriteName0' | Out-Null
    AwaitVisible 'FavoriteBusy0' | Out-Null
    Assert ((Name 'FavoriteLines0') -match 'RE 1') 'Collapsed favorite restores its persisted line inventory before provider completion'
    if ($ScreenshotDirectory) { Snapshot 'cache-start-slow-nearby-cached' 'cache-during-refresh' }
    Click 'ToggleFavorite0'
    Assert ((Name 'FavoriteDeparture0_0') -match 'RE 1 .*Stand 1') 'Cached future departure is visible before delayed provider completion'
    if ($ScreenshotDirectory) {
        $busyDuringRefresh = Find 'FavoriteBusy0'
        $scrollItem = $null
        if ($null -ne $busyDuringRefresh -and $busyDuringRefresh.TryGetCurrentPattern([System.Windows.Automation.ScrollItemPattern]::Pattern, [ref]$scrollItem)) { $scrollItem.ScrollIntoView() }
        if ($null -ne $busyDuringRefresh -and -not $busyDuringRefresh.Current.IsOffscreen) { Snapshot 'cache-start-slow-nearby-cached-expanded' 'cache-during-refresh-expanded' }
    }
    else {
        $started = AwaitCalls 1 3
        Assert ($started.TotalSeconds -lt 3) 'Startup provider refresh begins before the six-second Nearby lookup completes'
        Assert ((DepartureCalls) -eq 1) 'Exactly one startup provider refresh is requested for cached favorite'
    }
    for ($attempt = 0; $attempt -lt 100 -and (Find 'FavoriteBusy0').Current.IsOffscreen -eq $false; $attempt++) { Start-Sleep -Milliseconds 100 }
    Assert ((Name 'FavoriteDeparture0_0') -match 'RE 1 .*Live Stand 1') 'Delayed provider response replaces displayed cached departure'
    if ($ScreenshotDirectory) { Snapshot 'cache-start-slow-nearby-live' 'cache-after-refresh' }
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
