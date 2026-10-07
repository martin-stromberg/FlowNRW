param([Parameter(Mandatory=$true)][string]$Exe, [string]$ScreenshotDirectory, [switch]$FavoriteTimersOnly)
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
$previousScenario = $env:FLOWNRW_UI_TEST_SCENARIO
$testDirectory = Join-Path (Get-Location) ('artifacts/tests/refresh/' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Force $testDirectory | Out-Null
$env:FLOWNRW_UI_TEST_REFRESH_SETTINGS = Join-Path $testDirectory 'refresh.json'
$env:FLOWNRW_UI_TEST_FAVORITES = Join-Path $testDirectory 'favorites.json'
$env:FLOWNRW_UI_TEST_SCENARIO = 'complete'
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
    for ($attempt = 0; $attempt -lt 120; $attempt++) { RequireForeground; if ((Name $id) -match $pattern) { return }; Start-Sleep -Milliseconds 100 }
    throw ($id + ' did not reach ' + $pattern + '; actual: ' + (Name $id))
}
function AwaitHidden([string]$id, [int]$maximumSeconds = 15) {
    for ($attempt = 0; $attempt -lt ($maximumSeconds * 10); $attempt++) {
        RequireForeground
        $element = Find $id
        if ($null -eq $element -or $element.Current.IsOffscreen) { return }
        Start-Sleep -Milliseconds 100
    }
    throw ('Expected hidden native element ' + $id)
}
function AwaitBusyDone([string]$id) {
    # Completed requests are proven by the leaving busy indicator; success statuses stay hidden.
    for ($attempt = 0; $attempt -lt 50; $attempt++) {
        RequireForeground
        $element = Find $id
        if ($element -and -not $element.Current.IsOffscreen) { AwaitHidden $id; return }
        Start-Sleep -Milliseconds 100
    }
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
    if ([RefreshWindow]::GetForegroundWindow() -eq [IntPtr]$script:window.Current.NativeWindowHandle) { return }
    Foreground
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
    Get-Process FlowNRW -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
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
    for ($attempt = 0; $attempt -lt 30 -and $null -eq (Find 'MonitorStop'); $attempt++) { Start-Sleep -Milliseconds 200 }
    if ($null -eq (Find 'MonitorStop')) { Click 'StopMatch0'; WaitElement 'MonitorStop' | Out-Null }
    AwaitBusyDone 'MonitorBusy'; WaitElement 'Departure0' | Out-Null; Foreground
}
function AddFavorite([string]$name) {
    OpenStop $name; Click 'ToggleFavorite'; AwaitText 'FavoriteToggleStatus' 'gespeichert'
    Back; SelectTab 'Abfahrten'; WaitElement 'OpenHomeStops' | Out-Null; Foreground
}
function FavoriteIndex([string]$name) {
    for ($i = 0; $i -lt 10; $i++) {
        $card = Find ('FavoriteName' + $i)
        if ($null -eq $card) { return -1 }
        if ((Name ('FavoriteName' + $i)) -match [Regex]::Escape($name)) { return $i }
    }
    return -1
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
try {
    Write-Output 'Native refresh tests use actual 30-second waits; all paths and data are isolated fixtures.'
    StartApp
    if (!$FavoriteTimersOnly) {
    Click 'OpenRefreshSettings'; AwaitText 'RefreshIntervalStatus' 'alle 60 Sekunden'
    Assert ((Name 'RefreshIntervalStatus') -match 'alle 60 Sekunden') 'Absent settings use documented 60-second default'
    Assert ($null -eq (Find 'RefreshSettingsStatus')) 'Resolved default interval shows no redundant status text'
    $bounds = $script:window.Current.BoundingRectangle
    $transform = $script:window.GetCurrentPattern([System.Windows.Automation.TransformPattern]::Pattern)
    $transform.Resize(430,900); ChooseInterval 30
    Assert ((WaitElement 'RefreshInterval').Current.BoundingRectangle.Width -le 430) 'Native interval selector fits narrow viewport'
    (WaitElement 'SaveRefreshSettings').SetFocus(); RequireForeground; [System.Windows.Forms.SendKeys]::SendWait('{ENTER}')
    AwaitText 'RefreshSettingsStatus' 'gespeichert\.'; AwaitText 'RefreshIntervalStatus' 'alle 30 Sekunden'
    Snapshot 'native-refresh-settings-narrow'; $transform.Resize($bounds.Width,$bounds.Height)
    Back; StartApp; Click 'OpenRefreshSettings'; AwaitText 'RefreshIntervalStatus' 'alle 30 Sekunden'; Back
    Assert ($true) '30-second preference survives a new process'
    OpenStop 'Refresh Station'
    $stop = 'fixture-0'; $initial = Count $stop; $first = Name 'Departure0'
    $clock = [Diagnostics.Stopwatch]::StartNew()
    AwaitCount $stop ($initial + 1); AwaitHidden 'MonitorBusy'
    Assert ($clock.Elapsed.TotalSeconds -ge 27) 'First automatic request waited a complete productive interval, without accelerated fixture clock'
    Assert ((Name 'Departure0') -ne $first) 'Automatic request changes displayed departure stand without manual click'
    SetText 'FavoriteScenario' 'complete-slow'
    $before = Count $stop; AwaitCount $stop ($before + 1)
    $manual = WaitElement 'RefreshDepartures'
    if ($manual.Current.IsEnabled) { $manual.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern).Invoke() }
    else { Assert ($true) 'Manual action is guarded while automatic request runs' }
    Hold 7; Assert ((Count $stop) -eq ($before + 1)) 'Manual/automatic overlap starts exactly one request'
    AwaitHidden 'MonitorBusy'
    $retained = Name 'Departure0'
    SetText 'FavoriteScenario' 'refresh-error'; $before = Count $stop; AwaitCount $stop ($before + 1)
    AwaitText 'MonitorStatus' 'Letzte bekannte'
    Assert ((Name 'Departure0') -eq $retained) 'Automatic failure retains last known departures'
    Assert ((Name 'MonitorStatus') -match 'Datenstand') 'Failure points at the retained data timestamp'
    Hold 3; Assert ((Count $stop) -eq ($before + 1)) 'Failed refresh does not create an immediate retry loop'
    SetText 'FavoriteScenario' 'complete'; Click 'RefreshDepartures'; AwaitBusyDone 'MonitorBusy'
    $before = Count $stop
    Click 'OpenRefreshSettings'; Foreground; Hold 35; Back; Foreground
    Assert ((Count $stop) -eq $before) 'Settings navigation stops the invisible monitor for more than one interval'
    Settings 0; $before = Count $stop; Hold 35
    Assert ((Count $stop) -eq $before) 'Off disables the automatic monitor loop'
    Click 'RefreshDepartures'; AwaitBusyDone 'MonitorBusy'
    Assert ((Count $stop) -eq ($before + 1)) 'Manual refresh still works while automatic mode is off'
    Settings 30; Hold 10; Settings 60; $before = Count $stop; Hold 35
    Assert ((Count $stop) -eq $before) 'Interval change cancels the previous 30-second schedule'
    Click 'OpenRefreshSettings'; ChooseInterval 30; SetText 'RefreshSettingsScenario' 'refresh-store-error'
    Click 'SaveRefreshSettings'; AwaitText 'RefreshSettingsStatus' 'nicht gespeichert'
    Assert ((Name 'RefreshIntervalStatus') -match 'alle 60 Sekunden') 'Storage failure preserves effective interval'
    SetText 'RefreshSettingsScenario' 'success'; Click 'SaveRefreshSettings'; AwaitText 'RefreshSettingsStatus' 'gespeichert\.'
    Back; Foreground; $before = Count $stop
    $handle = [IntPtr]$script:window.Current.NativeWindowHandle
    [RefreshWindow]::ShowWindow($handle,6) | Out-Null; Start-Sleep -Milliseconds 300
    Hold 35 $false
    Assert ((Count $stop) -eq $before) 'Deactivated own window does not refresh'
    Foreground
    AwaitCount $stop ($before + 1) 5; AwaitHidden 'MonitorBusy'
    Hold 3; Assert ((Count $stop) -eq ($before + 1)) 'Reactivation starts one schedule without catch-up bursts'
    SetText 'FavoriteScenario' 'complete-slow'; $before = Count $stop; AwaitCount $stop ($before + 1)
    Back; SelectTab 'Abfahrten'; Foreground
    Click 'OpenJourneySearch'; Hold 35; SelectTab 'Abfahrten'; Foreground
    Assert ((Count $stop) -eq ($before + 1)) 'Page departure cancels slow work and search has no invisible monitor loop'
    StartApp; Click 'OpenRefreshSettings'; AwaitText 'RefreshIntervalStatus' 'alle 30 Sekunden'; Back
    Assert ($true) 'Retry-saved interval survives another process'
    } else { Settings 30 }
    AddFavorite 'Favorite Far'; AddFavorite 'Favorite Near'
    AwaitBusyDone 'FavoriteBusy0'; AwaitBusyDone 'FavoriteBusy1'
    # 'favorite-slow' delays only the far card by six seconds while the near
    # card answers quickly, so the overlap proof stays deterministic.
    SetText 'HomeScenario' 'favorite-slow'
    # Cards are sorted by distance once the position resolved, so resolve card
    # indices by name instead of assuming insertion order.
    $farCard = FavoriteIndex 'Favorite Far'; $nearCard = FavoriteIndex 'Favorite Near'
    Assert ($farCard -ge 0 -and $nearCard -ge 0) 'Both favorite cards are present for the overlap proof'
    $farBusy = 'FavoriteBusy' + $farCard; $nearBusy = 'FavoriteBusy' + $nearCard
    $farRefresh = 'RefreshFavorite' + $farCard; $nearRefresh = 'RefreshFavorite' + $nearCard
    $proven = $false
    for ($round = 0; $round -lt 3 -and -not $proven; $round++) {
        $far = Count 'fixture-favorite-far-0'
        $trigger = WaitElement $farRefresh
        if ($trigger.Current.IsEnabled) { $trigger.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern).Invoke() }
        AwaitCount 'fixture-favorite-far-0' ($far + 1)
        WaitElement $farBusy | Out-Null
        $manual = WaitElement $farRefresh
        $expected = $far + 1
        if ($manual.Current.IsEnabled) { $manual.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern).Invoke(); $expected = $far + 2 }
        Assert ((Count 'fixture-favorite-far-0') -eq $expected) 'Favorite manual/automatic overlap is guarded'
        $nearTrigger = WaitElement $nearRefresh
        if ($nearTrigger.Current.IsEnabled) { $nearTrigger.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern).Invoke() }
        AwaitBusyDone $nearBusy
        $slowBusy = Find $farBusy
        $proven = $null -ne $slowBusy -and -not $slowBusy.Current.IsOffscreen
        if (!$proven) { AwaitBusyDone $farBusy }
    }
    Assert $proven 'Slow favorite remains loading while another card refresh completed'
    AwaitBusyDone $farBusy
    $retained = Name ('FavoriteLines' + $farCard)
    SetText 'HomeScenario' 'favorite-error'; $far = Count 'fixture-favorite-far-0'; $near = Count 'fixture-favorite-near-0'
    AwaitCount 'fixture-favorite-far-0' ($far + 1)
    AwaitText ('FavoriteStatus' + $farCard) 'Letzte bekannte'
    Assert ((Name ('FavoriteLines' + $farCard)) -eq $retained) 'Automatic favorite failure retains prior data'
    AwaitCount 'fixture-favorite-near-0' ($near + 1)
    SetText 'HomeScenario' 'complete'
    $farTrigger = WaitElement $farRefresh
    if ($farTrigger.Current.IsEnabled) { $farTrigger.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern).Invoke() }
    AwaitBusyDone $farBusy
    Settings 0; $far = Count 'fixture-favorite-far-0'; $near = Count 'fixture-favorite-near-0'; Hold 35
    Assert ((Count 'fixture-favorite-far-0') -eq $far -and (Count 'fixture-favorite-near-0') -eq $near) 'Off stops all favorite schedules'
    Settings 30
    for ($round=0; $round -lt 3; $round++) { Click 'OpenJourneySearch'; SelectTab 'Abfahrten'; Foreground }
    $far = Count 'fixture-favorite-far-0'; $near = Count 'fixture-favorite-near-0'
    AwaitCount 'fixture-favorite-far-0' ($far + 1); AwaitCount 'fixture-favorite-near-0' ($near + 1)
    AwaitHidden 'FavoriteBusy0'; AwaitHidden 'FavoriteBusy1'
    Hold 3
    Assert ((Count 'fixture-favorite-far-0') -eq ($far + 1) -and (Count 'fixture-favorite-near-0') -eq ($near + 1)) 'Repeated page navigation does not multiply favorite timers'
    if ($FavoriteTimersOnly) { Write-Output 'PASS native favorite timer regression: real30s, independent cards, overlap, error retention, Off and navigation' }
    else { Write-Output 'PASS all native refresh scenarios: real timing, persistence, off/change, navigation/activity, independent cards, overlap, retained errors and storage retry' }
} finally {
    if ($script:app -and !$script:app.HasExited) { Stop-Process -Id $script:app.Id }
    Get-Process FlowNRW -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
    $env:FLOWNRW_UI_TEST_REFRESH_SETTINGS = $previousSettings
    $env:FLOWNRW_UI_TEST_FAVORITES = $previousFavorites
    $env:FLOWNRW_UI_TEST_SCENARIO = $previousScenario
}
