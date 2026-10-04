param([Parameter(Mandatory)][string]$Exe, [string]$ScreenshotDirectory = 'docs/help/design/verification/matrix', [ValidateSet('light','dark')][string]$Theme='light', [int]$Width=430, [int]$Height=900, [ValidateSet(100,150)][int]$TextScale=100, [string]$Scenario='success', [switch]$HomeOnly)
$ErrorActionPreference='Stop'
Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes
New-Item -ItemType Directory -Force $ScreenshotDirectory | Out-Null
$ScreenshotDirectory = (Resolve-Path $ScreenshotDirectory).Path
$run = Join-Path $env:TEMP ('flownrw-design-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory $run | Out-Null
$keys = @('FLOWNRW_UI_TEST_THEME','FLOWNRW_UI_TEST_TEXT_SCALE','FLOWNRW_UI_TEST_HIDE_CONTROLS','FLOWNRW_UI_TEST_SCENARIO','FLOWNRW_UI_TEST_FAVORITES','FLOWNRW_UI_TEST_REFRESH_SETTINGS')
$previous=@{}; foreach($key in $keys) { $previous[$key]=[Environment]::GetEnvironmentVariable($key) }
$env:FLOWNRW_UI_TEST_THEME=$Theme
$env:FLOWNRW_UI_TEST_TEXT_SCALE="$TextScale"
$env:FLOWNRW_UI_TEST_HIDE_CONTROLS='1'
$env:FLOWNRW_UI_TEST_SCENARIO=$Scenario
$env:FLOWNRW_UI_TEST_FAVORITES=Join-Path $run 'favorites.json'
$env:FLOWNRW_UI_TEST_REFRESH_SETTINGS=Join-Path $run 'refresh.json'
[IO.File]::WriteAllText($env:FLOWNRW_UI_TEST_REFRESH_SETTINGS,'0')
$app=Start-Process -FilePath $Exe -PassThru -WindowStyle Hidden
try {
    $condition=New-Object System.Windows.Automation.PropertyCondition([System.Windows.Automation.AutomationElement]::ProcessIdProperty,$app.Id)
    $script:window=$null
    for($attempt=0;$attempt -lt 100 -and !$script:window;$attempt++) { Start-Sleep -Milliseconds 200; $script:window=[System.Windows.Automation.AutomationElement]::RootElement.FindFirst([System.Windows.Automation.TreeScope]::Children,$condition) }
    if(!$script:window) { throw 'Native design window missing' }
    function Find([string]$id) {
        $c = New-Object System.Windows.Automation.PropertyCondition([System.Windows.Automation.AutomationElement]::AutomationIdProperty, $id)
        return $script:window.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $c)
    }
    function Wait([string]$id) {
        for ($i = 0; $i -lt 100; $i++) {
            $e = Find $id; if ($e) { return $e }
            if ($i -gt 5 -and $i % 5 -eq 0) {
                $all = $script:window.FindAll([System.Windows.Automation.TreeScope]::Descendants, [System.Windows.Automation.Condition]::TrueCondition)
                foreach ($element in $all) {
                    $scroll = $null
                    if ($element.TryGetCurrentPattern([System.Windows.Automation.ScrollPattern]::Pattern, [ref]$scroll) -and $scroll.Current.VerticallyScrollable) {
                        if ($scroll.Current.VerticalScrollPercent -ge 99) { $scroll.SetScrollPercent(-1,0) }
                        else { $scroll.Scroll([System.Windows.Automation.ScrollAmount]::NoAmount,[System.Windows.Automation.ScrollAmount]::LargeIncrement) }
                        break
                    }
                }
            }
            Start-Sleep -Milliseconds 100
        }
        Write-Output ('DIAGNOSTIC missing ' + $id + '; visible page markers: ' + ((@('OriginText','StopQuery','MonitorStop','MapCanvas','Journey0','JourneyDetailSection0') | Where-Object { $null -ne (Find $_) }) -join ', '))
        Snapshot ('failure-missing-' + $id)
        $script:window.FindAll([System.Windows.Automation.TreeScope]::Descendants, [System.Windows.Automation.Condition]::TrueCondition) | ForEach-Object { Write-Host ($_.Current.AutomationId + '|' + $_.Current.Name) }
        throw "Missing $id"
    }
    function Click([string]$id) {
        $target = Wait $id
        for ($attempt = 0; $attempt -lt 100 -and !$target.Current.IsEnabled; $attempt++) {
            Start-Sleep -Milliseconds 100; $target = Wait $id
        }
        if (!$target.Current.IsEnabled) { throw "Native action remains disabled: $id" }
        $bounds = $target.Current.BoundingRectangle; $viewport = $script:window.Current.BoundingRectangle
        if ($target.Current.IsOffscreen -or $bounds.Top -lt $viewport.Top -or $bounds.Bottom -gt $viewport.Bottom) {
            $scroll = $null
            if ($target.TryGetCurrentPattern([System.Windows.Automation.ScrollItemPattern]::Pattern, [ref]$scroll)) { $scroll.ScrollIntoView() }
            else { $target.SetFocus() }
            Start-Sleep -Milliseconds 150; $target = Wait $id
        }
        try { $target.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern).Invoke() }
        catch { throw ("Native Invoke failed for $id (enabled=" + $target.Current.IsEnabled + ', offscreen=' + $target.Current.IsOffscreen + '): ' + $_.Exception.Message) }
        Start-Sleep -Milliseconds 150
    }
    function SetText([string]$id, [string]$value) { (Wait $id).GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern).SetValue($value); Start-Sleep -Milliseconds 100 }
    function Name([string]$id) { return (Wait $id).Current.Name }
    function Assert([bool]$value, [string]$message) { if (!$value) { throw $message }; Write-Output "PASS $message" }
    function MarkerY([double]$latitude) {
        $view = (Name 'MapViewport').Split(';')
        $center = [double]::Parse($view[0], [Globalization.CultureInfo]::InvariantCulture)
        $zoom = [double]::Parse($view[2], [Globalization.CultureInfo]::InvariantCulture)
        $size = 256 * [Math]::Pow(2,$zoom)
        $targetY = (1 - [Math]::Log([Math]::Tan($latitude * [Math]::PI / 180) + 1 / [Math]::Cos($latitude * [Math]::PI / 180)) / [Math]::PI) / 2 * $size
        $centerY = (1 - [Math]::Log([Math]::Tan($center * [Math]::PI / 180) + 1 / [Math]::Cos($center * [Math]::PI / 180)) / [Math]::PI) / 2 * $size
        return 0.5 + ($targetY - $centerY) / (Wait 'MapCanvas').Current.BoundingRectangle.Height
    }
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
    function AssertNarrowAction([string]$id, [string]$message) {
        # Resize returns before WinUI has completed its layout pass.
        $width = 0
        for ($attempt = 0; $attempt -lt 50; $attempt++) {
            $width = (Wait $id).Current.BoundingRectangle.Width
            if ($width -gt 0 -and $width -le 430) { break }
            Start-Sleep -Milliseconds 100
        }
        Assert ($width -gt 0 -and $width -le 430) ($message + " (width=$width)")
    }
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
        $bitmap = New-Object Drawing.Bitmap(([int]$rect.Width), ([int]$rect.Height))
        $graphics = [Drawing.Graphics]::FromImage($bitmap)
        try {
            $dc = $graphics.GetHdc()
            try { if (![NativeWindowCapture]::PrintWindow($handle, $dc, 2)) { throw 'Own window capture unavailable' } }
            finally { $graphics.ReleaseHdc($dc) }
            $bitmap.Save((Join-Path $ScreenshotDirectory ($name + '.png')), [Drawing.Imaging.ImageFormat]::Png)
        } finally { $graphics.Dispose(); $bitmap.Dispose() }
    }
    Add-Type -AssemblyName System.Windows.Forms
    Add-Type @"
using System; using System.Runtime.InteropServices;
public static class DesignWindow {
 [DllImport("user32.dll")] public static extern uint GetDpiForWindow(IntPtr h);
 [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr h);
 [DllImport("user32.dll")] public static extern IntPtr GetForegroundWindow();
 [DllImport("user32.dll")] public static extern bool SetCursorPos(int x,int y);
 [DllImport("user32.dll")] public static extern void mouse_event(uint flags,uint dx,uint dy,int data,UIntPtr info);
}
"@
    $handle=[IntPtr]$script:window.Current.NativeWindowHandle
    [DesignWindow]::SetForegroundWindow($handle) | Out-Null
    $dpi=[DesignWindow]::GetDpiForWindow($handle)
    $transform=$script:window.GetCurrentPattern([System.Windows.Automation.TransformPattern]::Pattern)
    $transform.Move(20,20); $transform.Resize($Width*$dpi/96,$Height*$dpi/96)
    Start-Sleep -Milliseconds 700
    $prefix="$Theme-$Width-$Height-$TextScale-$Scenario"
    $commit=(git rev-parse HEAD).Trim()
    $statusLines = @(git status --porcelain)
    $trackedChanges = @($statusLines | Where-Object { $_ -notmatch '^\?\?' })
    $untrackedChanges = @($statusLines | Where-Object { $_ -match '^\?\?' })
    $workingTree = if ($trackedChanges.Count -gt 0) { 'tracked files modified' } elseif ($untrackedChanges.Count -gt 0) { 'tracked files clean; untracked files present' } else { 'clean' }
    $buildHash=(Get-FileHash (Join-Path (Split-Path $Exe) 'FlowNRW.dll') -Algorithm SHA256).Hash
    function AssertOwnForeground {
        if ([DesignWindow]::GetForegroundWindow() -ne $handle) { throw 'Physical input cancelled: app is not the foreground window.' }
    }
    function AssertTouchTarget([string]$id) {
        if ($id -eq 'OriginCoordinateMode') {
            $node = Wait $id
            for ($n=0; $n -lt 4 -and $node; $n++) {
                Write-Output ('TARGET ' + $node.Current.AutomationId + ' ' + $node.Current.ClassName + ' ' + $node.Current.BoundingRectangle)
                $node = [System.Windows.Automation.TreeWalker]::RawViewWalker.GetParent($node)
            }
        }
        $bounds = (Wait $id).Current.BoundingRectangle
        $logicalWidth = $bounds.Width * 96 / $dpi
        $logicalHeight = $bounds.Height * 96 / $dpi
        Assert ($logicalWidth -ge 44 -and $logicalHeight -ge 44) ("Touch target $id >=44x44 ($logicalWidth x $logicalHeight)")
    }
    function Capture([string]$name,[string]$reference,[string]$focus='') {
        [DesignWindow]::SetForegroundWindow($handle) | Out-Null
        Start-Sleep -Milliseconds 150
        AssertOwnForeground
        $area=$script:window.Current.BoundingRectangle
        [DesignWindow]::SetCursorPos([int]($area.Left+$area.Width/2),[int]($area.Top+300)) | Out-Null
        AssertOwnForeground
        [DesignWindow]::mouse_event(0x0800,0,0,24000,[UIntPtr]::Zero)
        Start-Sleep -Milliseconds 350
        $focus = switch ($name) {
            'journey-walk-transfer' { 'JourneyDetailSection2' }
            'stop-list' { 'StopMatch0' }
            'no-journeys' { 'RoutingStatus' }
            'routing-provider-error' { 'RoutingStatus' }
            'routing-loading' { 'RoutingBusy' }
            default { $focus }
        }
        if ($focus) {
            $visible = $false
            for ($scrollAttempt = 0; $scrollAttempt -lt 20; $scrollAttempt++) {
                $target = Wait $focus
                $targetBounds = $target.Current.BoundingRectangle
                if (!$target.Current.IsOffscreen -and $targetBounds.Top -ge $area.Top + 110 -and $targetBounds.Bottom -le $area.Bottom - 55) { $visible = $true; break }
                AssertOwnForeground
                [DesignWindow]::mouse_event(0x0800,0,0,-240,[UIntPtr]::Zero)
                Start-Sleep -Milliseconds 120
            }
            Assert $visible ('Screenshot state visible: ' + $focus)
        }
        Snapshot ($prefix+'-'+$name)
        $bounds=$script:window.Current.BoundingRectangle
        [pscustomobject]@{image=$prefix+'-'+$name+'.png';commit=$commit;workingTree=$workingTree;buildSha256=$buildHash;platform='Windows native MAUI';theme=$Theme;logicalWidth=$Width;logicalHeight=$Height;physicalWidth=$bounds.Width;physicalHeight=$bounds.Height;dpi=$dpi;textScale=$TextScale;scaleMethod='UiTest app text scaling';scenario=$name;reference=$reference;fixtureControls='hidden';timestamp=[DateTimeOffset]::Now.ToString('O')} | ConvertTo-Json -Compress | Add-Content (Join-Path $ScreenshotDirectory 'matrix.jsonl')
        Write-Output ('CAPTURE '+$name)
    }
    Wait 'OpenHomeStops' | Out-Null
    if ($env:FLOWNRW_DESIGN_SEARCH_ONLY -eq '1') {
        SelectTab 'Verbindungen'
        AssertTouchTarget 'OriginCoordinateMode'
        Capture 'search-form' 'verbindungssuche'
        return
    }
    Assert ($null -eq (Find 'HomeScenario')) 'Fixture controls hidden'
    Capture 'empty-favorites' 'abfahrtsmonitor_live'
    Click 'OpenRefreshSettings'
    (Wait 'SaveRefreshSettings').SetFocus()
    AssertTouchTarget 'SaveRefreshSettings'
    AssertTouchTarget 'RefreshInterval'
    Assert ((Wait 'SaveRefreshSettings').Current.HasKeyboardFocus) 'Save action has native keyboard focus'
    AssertOwnForeground
    [System.Windows.Forms.SendKeys]::SendWait('{ENTER}')
    if($Scenario -eq 'refresh-store-error') { Status 'RefreshSettingsStatus' 'nicht gespeichert|fehlgeschlagen'; Capture 'settings-error-keyboard' 'common-tokens'; return }
    Status 'RefreshSettingsStatus' 'gespeichert'
    Capture 'settings-success-keyboard' 'common-tokens'
    Back
    foreach($favorite in @('Favorite Far','Favorite Near','Favorite Missing')) {
        Click 'OpenHomeStops'; SetText 'StopQuery' $favorite; Click 'FindStops'; Click 'StopMatch0'; Wait 'Departure0' | Out-Null
        Assert ($null -eq (Find 'MonitorStatus')) 'Successful monitor refresh has no recurring status text'
        Assert ($null -eq (Find 'RefreshIntervalStatus')) 'Monitor does not repeat the automatic refresh interval'
        if($favorite -eq 'Favorite Far') {
            Capture 'monitor-delayed' 'abfahrtsmonitor_live'
            Capture 'monitor-unknown' 'abfahrtsmonitor_live' 'Departure2'
            Capture 'monitor-cancelled' 'abfahrtsmonitor_live' 'Departure3'
        }
        Click 'ToggleFavorite'; Status 'FavoriteToggleStatus' 'hinzugefügt|gespeichert'; Back; SelectTab 'Abfahrten'
    }
    Capture 'home-distance-unknown' 'abfahrtsmonitor_live' 'OpenHomeStops'
    Capture 'home-distance-unknown-second' 'abfahrtsmonitor_live' 'FavoriteDistance1'
    Capture 'home-distance-unknown-third' 'abfahrtsmonitor_live' 'FavoriteDistance2'
    Click 'ToggleFavorite0'; Capture 'home-favorite-expanded' 'abfahrtsmonitor_live' 'FavoriteDeparture0_0'
    Click 'ToggleFavorite0'; Capture 'home-favorite-collapsed' 'abfahrtsmonitor_live' 'FavoriteLines0'
    Click 'SortFavorites'; Start-Sleep -Milliseconds 500; Contains 'FavoriteDistance0' 'Luftlinie'; Contains 'HomeLocationStatus' 'fehlen Koordinaten'
    Capture 'home-distance-known' 'abfahrtsmonitor_live' 'OpenHomeStops'
    Capture 'home-distance-known-second' 'abfahrtsmonitor_live' 'FavoriteDistance1'
    Capture 'home-distance-known-third' 'abfahrtsmonitor_live' 'FavoriteDistance2'
    if ($HomeOnly) { return }
    SelectTab 'Haltestellen'; SetText 'StopQuery' 'Essen Hauptbahnhof'; Click 'FindStops'; Wait 'StopMatch1' | Out-Null
    Capture 'stop-list' 'umgebungskarte_stationen'
    Click 'StopMatch0'; Wait 'Departure0' | Out-Null; Capture 'stop-monitor-cached' 'abfahrtsmonitor_live' 'Departure0'; Back
    Wait 'StopMatch1' | Out-Null; Capture 'stop-list-returned' 'haltestellensuche' 'StopMatch1'
    Click 'ShowStopMap'; Status 'MapStatus' 'Basiskarte geladen'; Capture 'map-stations' 'umgebungskarte_stationen'
    Click 'ShowMapList'; Capture 'map-native-list' 'umgebungskarte_stationen'
    Click 'MapStation1'; Wait 'Departure0' | Out-Null; Capture 'map-selected-station' 'abfahrtsmonitor_live'; Back; Back
    SetText 'StopQuery' 'map-offline'; Click 'FindStops'; Click 'ShowStopMap'; Status 'MapStatus' 'fehlt oder ist veraltet'; Capture 'map-offline' 'umgebungskarte_stationen'; Back
    SetText 'StopQuery' 'map-missing'; Click 'FindStops'; Click 'ShowStopMap'; Capture 'map-no-position' 'umgebungskarte_stationen'; Back
    SetText 'StopQuery' 'monitor-error'; Click 'FindStops'; Click 'StopMatch0'; Status 'MonitorStatus' 'fehlgeschlagen|nicht geladen'; Capture 'monitor-provider-error' 'abfahrtsmonitor_live'; Back
    SelectTab 'Verbindungen'; Capture 'search-form' 'verbindungssuche'
    AssertTouchTarget 'OriginSearch'
    AssertTouchTarget 'OriginLocation'
    AssertTouchTarget 'OriginCoordinateMode'
    AssertTouchTarget 'SearchJourneys'
    Click 'OriginSearch'; Capture 'search-invalid' 'verbindungssuche'
    SelectEndpoint 'Origin' 'Essen Hauptbahnhof'; SelectEndpoint 'Destination' 'Düsseldorf Hauptbahnhof'; Capture 'search-selections' 'verbindungssuche'
    Click 'SwapEndpoints'; Capture 'search-swapped' 'verbindungssuche'; Click 'SwapEndpoints'
    Click 'SearchJourneys'; Wait 'Journey0' | Out-Null; Capture 'journey-results' 'verbindungssuche'
    Click 'ToggleConnectionFavorite'; Capture 'journey-favorite' 'verbindungssuche' 'ToggleConnectionFavorite'
    Click 'Journey0'; Wait 'JourneyDetailSection1' | Out-Null; Capture 'journey-details' 'fahrtbegleiter_detail'; Capture 'journey-walk-transfer' 'fahrtbegleiter_detail' 'ShowJourneyMap'
    Capture 'journey-walk-distance' 'fahrtbegleiter_detail' 'JourneyWalk2'
    Capture 'journey-following-leg' 'fahrtbegleiter_detail' 'JourneyDetailSection3'
    Contains 'JourneyLine3' '^Bus 10$'
    Contains 'JourneyOperator3' '^Betreiber: Fixture Bus$'
    Capture 'journey-transfer-summary' 'fahrtbegleiter_detail' 'JourneyDetailSection4'; Back
    Click 'Journey1'; Click 'ShowJourneyMap'; Contains 'MapDataStatus' 'Keine darstellbare Geometrie'; Capture 'journey-no-geometry' 'fahrtbegleiter_detail'; Back; Back; Back
    SelectEndpoint 'Origin' 'route-empty'; Click 'SearchJourneys'; Status 'RoutingStatus' 'Keine Verbindung'; Capture 'no-journeys' 'verbindungssuche'
    SelectEndpoint 'Origin' 'route-error'; Click 'SearchJourneys'; Status 'RoutingStatus' 'fehlgeschlagen'; Capture 'routing-provider-error' 'verbindungssuche'
    SelectEndpoint 'Origin' 'route-slow'; Click 'SearchJourneys'; Capture 'routing-loading' 'verbindungssuche'
    Write-Output 'PASS native design matrix sequence'
} finally {
    if(!$app.HasExited) { Stop-Process -Id $app.Id }
    foreach($key in $keys) { [Environment]::SetEnvironmentVariable($key,$previous[$key]) }
}
