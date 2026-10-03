param([Parameter(Mandatory=$true)][string]$Exe, [string]$ScreenshotDirectory)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes
Add-Type -AssemblyName System.Windows.Forms
Add-Type @"
using System;
using System.Runtime.InteropServices;
public static class RefreshWindow {
 [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr h);
 [DllImport("user32.dll")] public static extern IntPtr GetForegroundWindow();
 [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr h, int command);
}
"@
$previousSettings = $env:FLOWNRW_UI_TEST_REFRESH_SETTINGS
$previousFavorites = $env:FLOWNRW_UI_TEST_FAVORITES
$testDirectory = Join-Path (Get-Location) ('artifacts/tests/refresh/' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Force $testDirectory | Out-Null
$env:FLOWNRW_UI_TEST_REFRESH_SETTINGS = Join-Path $testDirectory 'refresh.json'
$env:FLOWNRW_UI_TEST_FAVORITES = Join-Path $testDirectory 'favorites.json'
$script:app = $null
$script:window = $null
function Assert([bool]$ok, [string]$message) { if (!$ok) { throw $message }; Write-Output ('PASS ' + $message) }
function Find([string]$id) {
    $condition = New-Object System.Windows.Automation.PropertyCondition([System.Windows.Automation.AutomationElement]::AutomationIdProperty, $id)
    return $script:window.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $condition)
}
function WaitElement([string]$id) {
    for ($attempt = 0; $attempt -lt 120; $attempt++) { $found = Find $id; if ($found) { return $found }; Start-Sleep -Milliseconds 100 }
    throw ('Missing native element ' + $id)
}
function Name([string]$id) { return (WaitElement $id).Current.Name }
function Click([string]$id) { (WaitElement $id).GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern).Invoke(); Start-Sleep -Milliseconds 150 }
function SetText([string]$id, [string]$value) { (WaitElement $id).GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern).SetValue($value); Start-Sleep -Milliseconds 100 }
function Back { Click 'NavigationViewBackButton' }
function SelectTab([string]$name) {
    $condition = New-Object System.Windows.Automation.PropertyCondition([System.Windows.Automation.AutomationElement]::NameProperty, $name)
    $items = $script:window.FindAll([System.Windows.Automation.TreeScope]::Descendants, $condition)
    foreach ($item in $items) {
        $selection = $null
        if ($item.TryGetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern, [ref]$selection)) { $selection.Select(); Start-Sleep -Milliseconds 300; return }
        $invoke = $null
        if ($item.TryGetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern, [ref]$invoke)) { $invoke.Invoke(); Start-Sleep -Milliseconds 300; return }
    }
    throw ('Persistent native tab not selectable: ' + $name)
}
function AwaitText([string]$id, [string]$pattern) {
    for ($attempt = 0; $attempt -lt 120; $attempt++) { if ((Name $id) -match $pattern) { return }; Start-Sleep -Milliseconds 100 }
    throw ($id + ' did not reach ' + $pattern + '; actual: ' + (Name $id))
}
function Foreground {
    $handle = [IntPtr]$script:window.Current.NativeWindowHandle
    [RefreshWindow]::ShowWindow($handle, 9) | Out-Null
    [RefreshWindow]::SetForegroundWindow($handle) | Out-Null
    Start-Sleep -Milliseconds 200
    if ([RefreshWindow]::GetForegroundWindow() -ne $handle) { $script:window.SetFocus(); Start-Sleep -Milliseconds 300 }
    if ([RefreshWindow]::GetForegroundWindow() -ne $handle) { throw 'Own test window must be foreground; timer test cannot continue reliably' }
}
function RequireForeground {
    if ([RefreshWindow]::GetForegroundWindow() -ne [IntPtr]$script:window.Current.NativeWindowHandle) { throw 'Foreground changed during active timer observation; no timer success claimed' }
}
function Hold([int]$seconds, [bool]$active = $true) {
    $clock = [Diagnostics.Stopwatch]::StartNew()
    $reported = -1
    while ($clock.Elapsed.TotalSeconds -lt $seconds) {
        if ($active) { RequireForeground }
        elseif ([RefreshWindow]::GetForegroundWindow() -eq [IntPtr]$script:window.Current.NativeWindowHandle) { throw 'Window unexpectedly reactivated during inactive observation' }
        $bucket = [int][Math]::Floor($clock.Elapsed.TotalSeconds / 5)
        if ($bucket -gt $reported) { Write-Output ('WAIT real time ' + [int]$clock.Elapsed.TotalSeconds + '/' + $seconds + ' seconds'); $reported = $bucket }
        Start-Sleep -Milliseconds 250
    }
}
function Count([string]$stop) {
    $text = Name 'FavoriteCalls'
    if ($text -match ('(?:^|;)' + [Regex]::Escape($stop) + '=(\d+)(?:;|$)')) { return [int]$Matches[1] }
    return 0
}
function AwaitCount([string]$stop, [int]$expected, [int]$maximumSeconds = 40) {
    $clock = [Diagnostics.Stopwatch]::StartNew(); $reported = -1
    while ((Count $stop) -lt $expected -and $clock.Elapsed.TotalSeconds -lt $maximumSeconds) {
        RequireForeground
        $bucket = [int][Math]::Floor($clock.Elapsed.TotalSeconds / 5)
        if ($bucket -gt $reported) { Write-Output ('WAIT next real timer request ' + [int]$clock.Elapsed.TotalSeconds + ' seconds'); $reported = $bucket }
        Start-Sleep -Milliseconds 200
    }
    if ((Count $stop) -ne $expected) { Write-Output ('DIAGNOSTIC actual counts: ' + (Name 'FavoriteCalls')); foreach ($id in @('MonitorStatus','RefreshIntervalStatus','RefreshForeground')) { if (Find $id) { Write-Output ($id + ': ' + (Name $id)) } } }
    Assert ((Count $stop) -eq $expected) ('Exactly expected request count for ' + $stop + ': ' + $expected)
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
    WaitElement 'OpenHomeStops' | Out-Null; Foreground
}
function ChooseInterval([int]$seconds) {
    $values = @(0,30,60,120,300); $labels = @('Aus','30 Sekunden','60 Sekunden','2 Minuten','5 Minuten')
    $index = [Array]::IndexOf($values,$seconds)
    if ($index -lt 0) { throw 'Unsupported test interval' }
    Foreground
    $picker = WaitElement 'RefreshInterval'; $picker.SetFocus(); RequireForeground
    $expand = $null
    if ($picker.TryGetCurrentPattern([System.Windows.Automation.ExpandCollapsePattern]::Pattern,[ref]$expand)) {
        $expand.Expand(); Start-Sleep -Milliseconds 200
        $condition = New-Object System.Windows.Automation.PropertyCondition([System.Windows.Automation.AutomationElement]::NameProperty, $labels[$index])
        $item = $script:window.FindFirst([System.Windows.Automation.TreeScope]::Descendants,$condition)
        $selection = $null
        if ($item -and $item.TryGetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern,[ref]$selection)) {
            $selection.Select(); $expand.Collapse(); Start-Sleep -Milliseconds 150; return
        }
    }
    RequireForeground
    [System.Windows.Forms.SendKeys]::SendWait('{HOME}' + ('{DOWN}' * $index) + '{ENTER}')
    Start-Sleep -Milliseconds 200
}
function Settings([int]$seconds) {
    Click 'OpenRefreshSettings'; WaitElement 'RefreshInterval' | Out-Null
    ChooseInterval $seconds; Click 'SaveRefreshSettings'; AwaitText 'RefreshSettingsStatus' 'gespeichert\.'
    Assert ((Name 'RefreshIntervalStatus') -match $(if ($seconds -eq 0) { 'Aus' } else { 'alle ' + $seconds + ' Sekunden' })) ('Effective saved interval ' + $seconds)
    Back; Foreground
}
function OpenStop([string]$query) {
    Click 'OpenHomeStops'; SetText 'StopQuery' $query; Click 'FindStops'; Click 'StopMatch0'
    AwaitText 'MonitorStatus' 'manuell aktualisiert'; Foreground
}
function AddFavorite([string]$name) {
    OpenStop $name; Click 'ToggleFavorite'; AwaitText 'FavoriteToggleStatus' 'gespeichert'
    Back; SelectTab 'Abfahrten'; WaitElement 'OpenHomeStops' | Out-Null; Foreground
}
function Snapshot([string]$name) {
    if (!$ScreenshotDirectory) { return }
    RequireForeground
    Add-Type -AssemblyName System.Drawing
    $rect = $script:window.Current.BoundingRectangle
    $bitmap = New-Object Drawing.Bitmap(([int]$rect.Width - 20),([int]$rect.Height - 20))
    $graphics = [Drawing.Graphics]::FromImage($bitmap)
    try {
        $graphics.CopyFromScreen(([int]$rect.Left + 10),([int]$rect.Top + 10),0,0,$bitmap.Size)
        $bitmap.Save((Join-Path $ScreenshotDirectory ($name + '.png')),[Drawing.Imaging.ImageFormat]::Png)
    } finally { $graphics.Dispose(); $bitmap.Dispose() }
}
function PauseAndResume([int]$seconds = 1) {
    [RefreshWindow]::ShowWindow([IntPtr]$script:window.Current.NativeWindowHandle,6) | Out-Null
    Start-Sleep -Milliseconds 300
    Hold $seconds $false
    Foreground
}
function RouteCount { return [int](Name 'RouteCalls') }
try {
    StartApp
    Settings 300
    SetText 'HomeScenario' 'resume-fresh'
    OpenStop 'Resume Station'
    $stop = 'fixture-0'; $before = Count $stop
    PauseAndResume
    Hold 2
    Assert ((Count $stop) -eq $before) 'Fresh monitor resumes without another request'
    $old = Name 'Departure0'
    PauseAndResume 35
    AwaitCount $stop ($before + 1) 5
    AwaitText 'MonitorStatus' 'automatisch aktualisiert'
    Assert ((Name 'Departure0') -ne $old) 'Expired monitor updates immediately on resume without waiting 300 seconds'
    Hold 2
    Assert ((Count $stop) -eq ($before + 1)) 'Resume produces no duplicate monitor request'
    SetText 'FavoriteScenario' 'success'
    Click 'RefreshDepartures'; AwaitText 'MonitorStatus' 'manuell aktualisiert'
    $old = Name 'Departure0'; $before = Count $stop
    SetText 'FavoriteScenario' 'refresh-error'
    PauseAndResume
    AwaitText 'MonitorStatus' 'Letzte bekannte'
    Assert ((Count $stop) -eq ($before + 1) -and (Name 'Departure0') -eq $old) 'Resume error retains visible previous departures'
    SetText 'FavoriteScenario' 'refresh-slow'
    $before = Count $stop; PauseAndResume; AwaitCount $stop ($before + 1) 5
    PauseAndResume; AwaitCount $stop ($before + 2) 5
    AwaitText 'MonitorStatus' 'automatisch aktualisiert'
    Assert ((Name 'Departure0') -match ('Stand ' + ($before + 2) + '\b')) 'Rapid reactivation displays only the newest slow request'
    Hold 2
    Assert ((Count $stop) -eq ($before + 2)) 'Rapid reactivation does not create another timer or request'
    $before = Count $stop; PauseAndResume; AwaitCount $stop ($before + 1) 5
    Back; Hold 7
    Assert ($null -ne (Find 'StopQuery') -and $null -eq (Find 'MonitorStop')) 'Leaving slow resume cannot reopen the old monitor'
    SelectTab 'Abfahrten'; Foreground
    SetText 'HomeScenario' 'success'
    AddFavorite 'Favorite Far'; AddFavorite 'Favorite Near'
    AwaitText 'FavoriteStatus0' 'aktualisiert'; AwaitText 'FavoriteStatus1' 'aktualisiert'
    SetText 'HomeScenario' 'favorite-slow'
    $far = Count 'fixture-favorite-far-0'; $near = Count 'fixture-favorite-near-0'
    PauseAndResume
    AwaitCount 'fixture-favorite-far-0' ($far + 1) 5
    AwaitText 'FavoriteStatus1' 'automatisch aktualisiert'
    Assert ((Name 'FavoriteStatus0') -match 'werden aktualisiert') 'Slow favorite does not block another resume card'
    AwaitText 'FavoriteStatus0' 'automatisch aktualisiert'
    $old = Name 'FavoriteDeparture0_0'
    SetText 'HomeScenario' 'favorite-error'; PauseAndResume
    AwaitText 'FavoriteStatus0' 'Letzte bekannte'
    AwaitText 'FavoriteStatus1' 'automatisch aktualisiert'
    Assert ((Name 'FavoriteDeparture0_0') -eq $old) 'Favorite resume failure retains old data'
    Settings 0
    $far = Count 'fixture-favorite-far-0'; $near = Count 'fixture-favorite-near-0'
    PauseAndResume; Hold 2
    Assert ((Count 'fixture-favorite-far-0') -eq $far -and (Count 'fixture-favorite-near-0') -eq $near) 'Off suppresses automatic favorite resume'
    SetText 'HomeScenario' 'success'
    Click 'RefreshFavorite0'; AwaitText 'FavoriteStatus0' 'manuell aktualisiert'
    Assert ((Count 'fixture-favorite-far-0') -eq ($far + 1)) 'Manual refresh remains usable with automatic resume off'
    Settings 300
    Click 'OpenJourneySearch'; SetText 'LocationScenario' 'resume-route-stale'
    SetText 'OriginText' 'Resume Origin'; Click 'OriginSearch'; Click 'OriginMatch0'
    SetText 'DestinationText' 'Resume Destination'; Click 'DestinationSearch'; Click 'DestinationMatch0'
    Click 'SearchJourneys'; AwaitText 'ResultsStatus' 'automatisch aktualisiert'
    $before = RouteCount; PauseAndResume
    AwaitText 'ResultsStatus' 'automatisch aktualisiert'
    Assert ((RouteCount) -eq ($before + 1) -and $null -ne (Find 'Journey0')) 'Visible route results resume once without navigation'
    Click 'Journey0'; AwaitText 'DetailStatus' 'automatisch aktualisiert'
    $before = RouteCount; SetText 'LifecycleScenario' 'resume-route-error'; PauseAndResume
    AwaitText 'DetailStatus' 'Letzte bekannte'
    Assert ((RouteCount) -eq ($before + 1) -and (Name 'JourneyDetailSection0') -notmatch 'Keine Verbindung') 'Journey details retain data and page on resume failure'
    SetText 'LifecycleScenario' 'resume-route-stale'; PauseAndResume
    AwaitText 'DetailStatus' 'automatisch aktualisiert'
    Assert ((WaitElement 'ShowJourneyMap').Current.IsEnabled) 'Uniquely identified journey remains selected after resume'
    SetText 'LifecycleScenario' 'resume-route-fresh'; PauseAndResume
    AwaitText 'DetailStatus' 'automatisch aktualisiert'
    $before = RouteCount; PauseAndResume; Hold 2
    Assert ((RouteCount) -eq $before) 'Fresh journey details resume without another provider request'
    SetText 'LifecycleScenario' 'resume-route-stale'
    PauseAndResume 35
    AwaitText 'DetailStatus' 'automatisch aktualisiert'
    SetText 'LifecycleScenario' 'resume-route-missing'; PauseAndResume
    AwaitText 'DetailStatus' 'nicht mehr eindeutig'
    Assert (!(WaitElement 'ShowJourneyMap').Current.IsEnabled) 'Missing prior journey is not replaced silently'
    Snapshot 'native-lifecycle-detail'
    Write-Output 'PASS native lifecycle: fresh/expired monitor, failure retention, cancellation, independent favorites, off/manual, results and detail identity'
} finally {
    if ($script:app -and !$script:app.HasExited) { Stop-Process -Id $script:app.Id }
    $env:FLOWNRW_UI_TEST_REFRESH_SETTINGS = $previousSettings
    $env:FLOWNRW_UI_TEST_FAVORITES = $previousFavorites
}
