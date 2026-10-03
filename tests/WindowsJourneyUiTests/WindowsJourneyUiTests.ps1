param([string]$Exe, [switch]$Inspect, [string]$ScreenshotDirectory, [switch]$Monitors, [switch]$LiveMonitors, [switch]$Maps, [switch]$LiveMaps, [switch]$Locations, [switch]$LiveLocations, [switch]$Favorites)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes
$previousFavoritePath = $env:FLOWNRW_UI_TEST_FAVORITES
$previousRefreshPath = $env:FLOWNRW_UI_TEST_REFRESH_SETTINGS
$refreshTestDirectory = Join-Path (Get-Location) ('artifacts/tests/refresh-regression/' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $refreshTestDirectory -Force | Out-Null
$env:FLOWNRW_UI_TEST_REFRESH_SETTINGS = Join-Path $refreshTestDirectory 'refresh-settings.json'
[IO.File]::WriteAllText($env:FLOWNRW_UI_TEST_REFRESH_SETTINGS, '0')
if ($Favorites) {
    $testDirectory = Join-Path (Get-Location) ('artifacts/tests/favorites/' + [Guid]::NewGuid().ToString('N'))
    New-Item -ItemType Directory -Path $testDirectory -Force | Out-Null
    $env:FLOWNRW_UI_TEST_FAVORITES = Join-Path $testDirectory 'favorites.json'
}
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
    Wait 'OpenJourneySearch' | Out-Null
    if (!$Favorites) { Click 'OpenJourneySearch'; Wait 'OriginText' | Out-Null }
    if (!$LiveLocations -and !$Favorites) { Snapshot 'native-search' }
    if ($Favorites) {
        function FavoriteCount([int]$expected) { Status 'FavoriteCount' ('\b' + $expected + '\b'); Assert ($true) ('Favorite count is ' + $expected) }
        function HomeReady { Wait 'OpenHomeStops' | Out-Null; Wait 'HomeStatus' | Out-Null; Start-Sleep -Milliseconds 500 }
        function BackHome { Back; SelectTab 'Abfahrten'; HomeReady }
        function FindFavoriteMonitor([string]$query) {
            Click 'OpenHomeStops'; SetText 'StopQuery' $query; Click 'FindStops'; Click 'StopMatch0'
            Status 'MonitorStatus' 'manuell aktualisiert'
        }
        function AddFavorite([string]$query) {
            FindFavoriteMonitor $query
            Click 'ToggleFavorite'; Status 'FavoriteToggleStatus' 'hinzugefügt|gespeichert'
            BackHome
        }
        function FavoriteIndex([string]$name) {
            for ($index = 0; $index -lt 100; $index++) {
                $element = Find ('FavoriteName' + $index)
                if (!$element) { break }
                if ($element.Current.Name -eq $name) { return $index }
            }
            throw ('Favorite card missing: ' + $name)
        }
        function DepartureCount([string]$id) {
            $counter = Name 'FavoriteCalls'
            if ($counter -match ('(?:^|;)' + [Regex]::Escape($id) + '=(\d+)(?:;|$)')) { return [int]$Matches[1] }
            return 0
        }
        function RestartFavorites {
            if (!$script:app.HasExited) { Stop-Process -Id $script:app.Id; $script:app.WaitForExit() }
            $script:app = Start-Process -FilePath $Exe -PassThru -WindowStyle Hidden
            $condition = New-Object System.Windows.Automation.PropertyCondition([System.Windows.Automation.AutomationElement]::ProcessIdProperty, $script:app.Id)
            $script:window = $null
            for ($attempt = 0; $attempt -lt 100 -and !$script:window; $attempt++) {
                Start-Sleep -Milliseconds 200
                $script:window = [System.Windows.Automation.AutomationElement]::RootElement.FindFirst([System.Windows.Automation.TreeScope]::Children, $condition)
            }
            if (!$script:window) { throw 'Restarted favorite fixture window missing' }
            HomeReady
            Write-Output 'PASS new application process opened the same isolated favorite file'
        }
        HomeReady; FavoriteCount 0
        SetText 'HomeScenario' 'success'; Wait 'NearbyStop0' | Out-Null
        Click 'NearbyStop0'; Status 'MonitorStatus' 'manuell aktualisiert'
        Contains 'MonitorStop' 'Umgebung Süd'; Contains 'MonitorMetadata' 'fixture-nearby-0'
        BackHome
        Write-Output 'PASS home nearby selection opens its exact departure monitor'
        Contains 'HomeStatus' 'Keine Favoriten|keine Favoriten|Haltestelle'
        Assert ((Wait 'OpenHomeStops').Current.IsEnabled) 'Empty home offers actual stop lookup'
        FindFavoriteMonitor 'Favorite Far'
        SetText 'FavoriteScenario' 'store-error'; Click 'ToggleFavorite'
        Status 'FavoriteToggleStatus' 'fehlgeschlagen|nicht gespeichert|Speicherfehler'
        Contains 'ToggleFavorite' 'hinzufügen|speichern'
        SetText 'FavoriteScenario' 'success'; Click 'ToggleFavorite'; Status 'FavoriteToggleStatus' 'hinzugefügt|gespeichert'
        BackHome; FavoriteCount 1
        Contains 'FavoriteName0' 'Favorite Far'; Status 'FavoriteStatus0' 'manuell aktualisiert'
        Contains 'FavoriteDeparture0_0' 'RE 1'; Contains 'FavoriteMetadata0' 'Quelle:.*Datenalter:.*Fallback'
        Contains 'FavoriteDistance0' 'unbekannt'
        FindFavoriteMonitor 'Favorite Far'; Contains 'ToggleFavorite' 'entfernen'
        BackHome; FavoriteCount 1
        RestartFavorites; FavoriteCount 1; Contains 'FavoriteName0' 'Favorite Far'
        Contains 'FavoriteDistance0' 'unbekannt'
        Write-Output 'PASS failed add is not reported as saved; retry, duplicate recognition and process persistence'
        AddFavorite 'Favorite Near'; FavoriteCount 2
        AddFavorite 'Favorite Missing'; FavoriteCount 3
        Contains 'FavoriteName0' 'Favorite Far'; Contains 'FavoriteName1' 'Favorite Near'; Contains 'FavoriteName2' 'Favorite Missing'
        SetText 'HomeScenario' 'success'; Click 'SortFavorites'; Status 'HomeLocationStatus' 'sortiert|aktualisiert|Entfernung'
        Status 'FavoriteName0' 'Favorite Near'; Contains 'FavoriteName1' 'Favorite Far'; Contains 'FavoriteName2' 'Favorite Missing'
        Assert ((Name 'FavoriteDistance0') -notmatch 'unbekannt') 'Known favorite coordinates provide actual calculated distance'
        Contains 'FavoriteDistance2' 'unbekannt'
        SetText 'HomeScenario' 'denied'; Click 'SortFavorites'; Status 'HomeLocationStatus' 'nicht erlaubt'
        Contains 'FavoriteName0' 'Favorite Far'; Contains 'FavoriteName1' 'Favorite Near'; Contains 'FavoriteDistance0' 'unbekannt'
        SetText 'HomeScenario' 'unavailable'; Click 'SortFavorites'; Status 'HomeLocationStatus' 'Keine aktuelle Position'
        Contains 'FavoriteName0' 'Favorite Far'; Contains 'FavoriteDistance1' 'unbekannt'
        Write-Output 'PASS explicit distance sorting, unknown positions and stable fallback order without location'
        SetText 'HomeScenario' 'success'
        $far = FavoriteIndex 'Favorite Far'; $near = FavoriteIndex 'Favorite Near'
        Status ('FavoriteStatus' + $far) 'manuell aktualisiert'
        $beforeFar = DepartureCount 'fixture-favorite-far-0'; $beforeNear = DepartureCount 'fixture-favorite-near-0'
        SetText 'HomeScenario' 'favorite-slow'
        Click ('RefreshFavorite' + $far)
        $repeat = Wait ('RefreshFavorite' + $far)
        if ($repeat.Current.IsEnabled) { $repeat.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern).Invoke() }
        Assert ((DepartureCount 'fixture-favorite-far-0') -eq ($beforeFar + 1)) 'Double refresh starts one request for the busy card'
        Click ('RefreshFavorite' + $near)
        Status ('FavoriteStatus' + $near) 'manuell aktualisiert'
        Assert ((DepartureCount 'fixture-favorite-near-0') -eq ($beforeNear + 1)) 'Another favorite refreshes independently while first is loading'
        Status ('FavoriteStatus' + $far) 'manuell aktualisiert'
        $retained = Name ('FavoriteDeparture' + $far + '_0')
        SetText 'HomeScenario' 'favorite-error'; Click ('RefreshFavorite' + $far)
        Status ('FavoriteStatus' + $far) 'Letzte bekannte|letzte bekannte|fehlgeschlagen'
        Assert ((Name ('FavoriteDeparture' + $far + '_0')) -eq $retained) 'Failed card refresh keeps last known departures'
        Click ('RefreshFavorite' + $near); Status ('FavoriteStatus' + $near) 'manuell aktualisiert'
        SetText 'HomeScenario' 'success'; Click ('RefreshFavorite' + $far); Status ('FavoriteStatus' + $far) 'manuell aktualisiert'
        Click ('OpenFavorite' + $far); Status 'MonitorStatus' 'manuell aktualisiert'; Contains 'MonitorStop' 'Favorite Far'; Back; HomeReady
        Click 'HomeMap'; Status 'MapDataStatus' '3 Favoriten.*2 Kartenpositionen'; Click 'ShowMapList'; Click 'MapStation0'
        Status 'MonitorStatus' 'manuell aktualisiert'; Contains 'MonitorStop' 'Favorite Far'; Back; Back; HomeReady
        Click 'OpenJourneySearch'; SelectEndpoint 'Origin' 'Essen'; SelectEndpoint 'Destination' 'Berlin'
        Click 'SearchJourneys'; Click 'Journey0'; Wait 'JourneyDetailSection1' | Out-Null
        Back; Back; SelectTab 'Abfahrten'; HomeReady
        Write-Output 'PASS independent monitor refreshes, failure retention and monitor/map/routing navigation'
        Add-Type @"
using System;
using System.Runtime.InteropServices;
public static class FavoriteKeyboard {
 [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr h);
 [DllImport("user32.dll")] public static extern IntPtr GetForegroundWindow();
}
"@
        Add-Type -AssemblyName System.Windows.Forms
        $bounds = $script:window.Current.BoundingRectangle
        $transform = $script:window.GetCurrentPattern([System.Windows.Automation.TransformPattern]::Pattern)
        $transform.Resize(430,900)
        $handle = [IntPtr]$script:window.Current.NativeWindowHandle
        [FavoriteKeyboard]::SetForegroundWindow($handle) | Out-Null
        (Wait 'SortFavorites').SetFocus(); Start-Sleep -Milliseconds 200
        if ([FavoriteKeyboard]::GetForegroundWindow() -ne $handle) { throw 'Own favorite window must be foreground for keyboard input' }
        [System.Windows.Forms.SendKeys]::SendWait('{ENTER}')
        Status 'HomeLocationStatus' 'sortiert|aktualisiert|Entfernung'
        Assert ((Wait 'SortFavorites').Current.BoundingRectangle.Width -le 430) 'Favorite sorting control fits narrow viewport and accepts keyboard'
        Snapshot 'native-favorites-narrow'; $transform.Resize($bounds.Width,$bounds.Height)
        SetText 'HomeScenario' 'store-error'; $missing = FavoriteIndex 'Favorite Missing'
        Click ('RemoveFavorite' + $missing); Status 'HomeStatus' 'fehlgeschlagen|nicht gespeichert|nicht entfernt|Speicherfehler'
        FavoriteCount 3; Assert ((FavoriteIndex 'Favorite Missing') -ge 0) 'Failed remove keeps saved favorite visible'
        SetText 'HomeScenario' 'success'; Click ('RemoveFavorite' + $missing); FavoriteCount 2
        RestartFavorites; FavoriteCount 2
        Assert ($null -eq (Find 'FavoriteName2')) 'Successful remove persists into a new process'
        Contains 'FavoriteName0' 'Favorite Far'; Contains 'FavoriteName1' 'Favorite Near'
        Status 'FavoriteStatus0' 'manuell aktualisiert'; Status 'FavoriteStatus1' 'manuell aktualisiert'
        SetText 'HomeScenario' 'favorite-slow'; Click 'RefreshFavorite0'; Status 'FavoriteStatus0' 'werden aktualisiert'
        Write-Output ('DIAGNOSTIC before removal: ' + (Name 'HomeStatus') + '; calls=' + (Name 'FavoriteCalls'))
        $remove = Wait 'RemoveFavorite0'
        Write-Output ('DIAGNOSTIC remove enabled=' + $remove.Current.IsEnabled + '; offscreen=' + $remove.Current.IsOffscreen + '; bounds=' + $remove.Current.BoundingRectangle)
        Click 'RemoveFavorite0'
        Start-Sleep -Milliseconds 500
        Write-Output ('DIAGNOSTIC after removal: ' + (Name 'HomeStatus') + '; count=' + (Name 'FavoriteCount'))
        FavoriteCount 1
        Start-Sleep -Seconds 6
        Contains 'FavoriteName0' 'Favorite Near'; Assert ($null -eq (Find 'FavoriteName1')) 'Late removed-card response cannot recreate favorite'
        SetText 'HomeScenario' 'success'; Click 'RemoveFavorite0'; FavoriteCount 0
        RestartFavorites; FavoriteCount 0; Contains 'HomeStatus' 'Keine Favoriten|keine Favoriten|Haltestelle'
        Write-Output 'PASS native favorites: add/failure/retry, duplicate, four process starts, sorting/fallback, independent refresh, removal/failure/retry, empty home, navigation and keyboard'
    } elseif ($LiveLocations) {
        # Never capture screenshots, raw UI names, endpoint values or returned stop identities here.
        Assert ($null -eq (Find 'LocationScenario')) 'Release has no fixture scenario control'
        Click 'OriginLocation'
        $resolved = $false
        for ($attempt = 0; $attempt -lt 180; $attempt++) {
            $state = Name 'OriginLocationStatus'
            if ($state -notmatch 'wird ermittelt') { $resolved = $true; break }
            Start-Sleep -Milliseconds 500
        }
        if (!$resolved) { throw 'OS location probe did not finish; permission dialog or operating-system response requires inspection without logging private UI content' }
        $success = (Name 'OriginLocationStatus') -match 'übernommen'
        if ($success) {
            Assert ((Name 'OriginSelection') -match 'Aktueller Standort') 'OS location accepted as endpoint (coordinates omitted)'
            Click 'OpenStopSearch'; Click 'FindNearbyStops'
            for ($attempt = 0; $attempt -lt 180; $attempt++) {
                $state = Name 'NearbyStatus'
                if ($state -notmatch 'wird ermittelt|werden geladen') { break }
                Start-Sleep -Milliseconds 500
            }
            if (Find 'StopMatch0') {
                Click 'StopMatch0'
                for ($attempt = 0; $attempt -lt 120 -and (Name 'MonitorStatus') -match 'werden'; $attempt++) { Start-Sleep -Milliseconds 500 }
                Assert ($null -ne (Find 'MonitorStop')) 'Real nearby candidate opens monitor (identity omitted)'
                Write-Output 'OS RESULT current position and real nearby selection available; provider departure status not disclosed'
                Back
            } else { Write-Output 'OS LIMIT location obtained; nearby request returned no selectable result; inspect sanitized provider diagnostics separately' }
            Back
        } else {
            $classification = if ($state -match 'nicht erlaubt') { 'permission denied' }
                elseif ($state -match 'deaktiviert') { 'location services disabled' }
                elseif ($state -match 'nicht unterstützt') { 'unsupported' }
                elseif ($state -match 'zu lange|Zeit') { 'timeout' }
                elseif ($state -match 'Keine aktuelle|nicht verfügbar') { 'position unavailable' }
                else { 'location request failed' }
            Write-Output ('OS LIMIT ' + $classification + '; successful position and nearby OS flow NOT EXECUTED')
            Assert ((Wait 'OriginText').Current.IsEnabled -and (Wait 'OriginSearch').Current.IsEnabled) 'Manual inputs remain enabled after actual OS outcome'
        }
        Write-Output 'OS probe finished without storing coordinates, nearby identities or screenshots; this is separate from fixture coverage'
    } elseif ($Locations) {
        Add-Type @"
using System;
using System.Runtime.InteropServices;
public static class LocationPointer {
 [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr h);
 [DllImport("user32.dll")] public static extern IntPtr GetForegroundWindow();
 [DllImport("user32.dll")] public static extern bool SetCursorPos(int x,int y);
 [DllImport("user32.dll")] public static extern void mouse_event(uint flags,uint dx,uint dy,uint data,UIntPtr info);
}
"@
        function LocationForeground {
            $handle = [IntPtr]$script:window.Current.NativeWindowHandle
            [LocationPointer]::SetForegroundWindow($handle) | Out-Null
            Start-Sleep -Milliseconds 200
            if ([LocationPointer]::GetForegroundWindow() -ne $handle) { $script:window.SetFocus(); Start-Sleep -Milliseconds 300 }
            if ([LocationPointer]::GetForegroundWindow() -ne $handle) { throw 'Own location window must be foreground for physical input' }
        }
        function LocationMapMarker {
            LocationForeground
            $rect = (Wait 'MapCanvas').Current.BoundingRectangle
            [LocationPointer]::SetCursorPos([int]($rect.Left + $rect.Width * 0.5), [int]($rect.Top + $rect.Height * (MarkerY 51.46))) | Out-Null
            [LocationPointer]::mouse_event(2,0,0,0,[UIntPtr]::Zero)
            [LocationPointer]::mouse_event(4,0,0,0,[UIntPtr]::Zero)
            Start-Sleep -Milliseconds 500
        }
        function LocationKeyboard([string]$id) {
            Add-Type -AssemblyName System.Windows.Forms
            LocationForeground
            (Wait $id).SetFocus()
            if ([LocationPointer]::GetForegroundWindow() -ne [IntPtr]$script:window.Current.NativeWindowHandle) { throw 'Own location window must retain foreground for keyboard input' }
            [System.Windows.Forms.SendKeys]::SendWait('{ENTER}')
            Start-Sleep -Milliseconds 200
        }
        function Locate([string]$prefix, [string]$scenario = 'success') {
            SetText 'LocationScenario' $scenario
            Click ($prefix + 'Location')
            Status ($prefix + 'LocationStatus') 'übernommen'
        }
        function Nearby([string]$scenario = 'success') {
            SetText 'NearbyScenario' $scenario
            Click 'FindNearbyStops'
            Status 'NearbyStatus' 'nahe Haltestellen gefunden'
        }
        function LocationRoundTrip {
            Click 'SearchJourneys'; Click 'Journey0'; Wait 'JourneyDetailSection1' | Out-Null; Back; Back
        }
        Contains 'LocationCalls' '^0$'
        SelectEndpoint 'Origin' 'Manual Start'; SelectEndpoint 'Destination' 'Manual Ziel'
        LocationRoundTrip; Contains 'LocationCalls' '^0$'
        Write-Output 'PASS manual route and navigation do not request location'
        Locate 'Origin'; Contains 'OriginSelection' 'Aktueller Standort'; LocationRoundTrip
        SelectEndpoint 'Origin' 'Manual start for located destination'
        Locate 'Destination'; Contains 'DestinationSelection' 'Aktueller Standort'; LocationRoundTrip
        SelectEndpoint 'Destination' 'Manual destination for location failures'
        Contains 'LocationCalls' '^2$'
        Locate 'Origin' 'reduced'; Contains 'OriginLocationStatus' 'ungefähr|Genauigkeit|ungenau'
        foreach ($failure in @(
            @('denied', 'nicht erlaubt'), @('disabled', 'deaktiviert'), @('unsupported', 'nicht unterstützt'),
            @('timeout', 'zu lange'), @('unavailable', 'Keine aktuelle Position'), @('error', 'nicht ermittelt')
        )) {
            $retained = Name 'OriginSelection'
            SetText 'LocationScenario' $failure[0]; Click 'OriginLocation'; Status 'OriginLocationStatus' $failure[1]
            Assert ((Name 'OriginSelection') -eq $retained) ('Failed location retains completed endpoint: ' + $failure[0])
            Assert ((Wait 'OriginLocation').Current.IsEnabled) ('Location retry enabled: ' + $failure[0])
            SelectEndpoint 'Origin' ('Manual after ' + $failure[0]); LocationRoundTrip
        }
        Locate 'Origin'
        SetText 'LocationScenario' 'denied'; Click 'OriginLocation'; Status 'OriginLocationStatus' 'nicht erlaubt'
        SelectEndpoint 'Origin' 'After revoked permission'; LocationRoundTrip
        Write-Output 'PASS fixture permission revocation, failure states, retained selection and manual recovery'
        SetText 'LocationScenario' 'slow'; Click 'OriginLocation'; Status 'OriginLocationStatus' 'wird ermittelt'
        SelectEndpoint 'Origin' 'Newest manual endpoint'
        Start-Sleep -Seconds 6
        Contains 'OriginSelection' 'Newest manual endpoint'
        SetText 'LocationScenario' 'slow'; Click 'DestinationLocation'; Status 'DestinationLocationStatus' 'wird ermittelt'
        Click 'OpenStopSearch'; SelectTab 'Verbindungen'
        Start-Sleep -Seconds 6
        Assert ((Name 'DestinationLocationStatus') -notmatch 'wird ermittelt') 'Page exit cancels pending location state'
        SetText 'DestinationText' 'After back'; SelectEndpoint 'Destination' 'After back'
        Locate 'Destination'; LocationRoundTrip
        Write-Output 'PASS late endpoint responses and page exit do not overwrite new input'
        $bounds = $script:window.Current.BoundingRectangle
        $transform = $script:window.GetCurrentPattern([System.Windows.Automation.TransformPattern]::Pattern)
        $transform.Resize(430,900)
        SetText 'LocationScenario' 'success'; LocationKeyboard 'OriginLocation'; Status 'OriginLocationStatus' 'übernommen'
        AssertNarrowAction 'OriginLocation' 'Location action fits narrow viewport'
        Snapshot 'native-location-narrow'
        $transform.Resize($bounds.Width,$bounds.Height)
        Click 'OpenStopSearch'
        Nearby
        Contains 'StopMatch0' 'Umgebung Süd'; Contains 'StopMatch0' '225'
        Contains 'StopMatch1' 'Umgebung Nord'; Contains 'StopMatch1' 'unbekannt'
        Contains 'StopSearchMetadata' 'Nearby.*Datenalter:.*Fallback'
        $bounds = $script:window.Current.BoundingRectangle
        $transform = $script:window.GetCurrentPattern([System.Windows.Automation.TransformPattern]::Pattern)
        $transform.Resize(430,900)
        AssertNarrowAction 'FindNearbyStops' 'Nearby action fits narrow viewport'
        Snapshot 'native-nearby-narrow'
        $transform.Resize($bounds.Width,$bounds.Height)
        Click 'StopMatch0'; Status 'MonitorStatus' 'manuell aktualisiert'; Contains 'MonitorMetadata' 'fixture-nearby-0'
        Back; Click 'ShowStopMap'; Status 'MapStatus' 'Basiskarte geladen\.'
        Contains 'MapDataStatus' '2 Haltestellen.*2 Kartenpositionen'
        LocationMapMarker
        Status 'MonitorStatus' 'manuell aktualisiert'; Contains 'MonitorMetadata' 'fixture-nearby-1'
        Back; Click 'ShowMapList'; LocationKeyboard 'MapStation0'
        Status 'MonitorStatus' 'manuell aktualisiert'; Contains 'MonitorMetadata' 'fixture-nearby-0'
        Back; Back
        Nearby 'nearby-missing'; Click 'ShowStopMap'
        Contains 'MapDataStatus' '2 Haltestellen.*1 Kartenpositionen'
        Click 'ShowMapList'; Contains 'MapStation1' 'Keine Kartenposition'; Click 'MapStation1'
        Status 'MonitorStatus' 'manuell aktualisiert'; Contains 'MonitorMetadata' 'fixture-nearby-1'; Back; Back
        SetText 'NearbyScenario' 'nearby-empty'; Click 'FindNearbyStops'; Status 'NearbyStatus' 'Keine Haltestellen'
        Assert ($null -eq (Find 'StopMatch0')) 'Successful empty nearby result removes previous candidates'
        Nearby
        SetText 'NearbyScenario' 'nearby-error'; Click 'FindNearbyStops'; Status 'NearbyStatus' 'nicht geladen|fehlgeschlagen'
        if (Find 'StopMatch0') { Contains 'NearbyStatus' 'vorher|bisher|letzte|bekannt' }
        SetText 'StopQuery' 'Manual after nearby failure'; Click 'FindStops'; Click 'StopMatch0'
        Status 'MonitorStatus' 'manuell aktualisiert'; Contains 'MonitorStop' 'Manual after nearby failure'; Back
        foreach ($failure in @(@('denied','nicht erlaubt'), @('timeout','zu lange'), @('unavailable','Keine aktuelle Position'))) {
            SetText 'NearbyScenario' $failure[0]; Click 'FindNearbyStops'; Status 'NearbyStatus' $failure[1]
            Assert ((Wait 'FindStops').Current.IsEnabled) ('Manual stop lookup enabled after ' + $failure[0])
        }
        SetText 'NearbyScenario' 'slow'; Click 'FindNearbyStops'; Status 'NearbyStatus' 'wird ermittelt'
        SetText 'StopQuery' 'Newest manual stop'; Click 'FindStops'; Wait 'StopMatch0' | Out-Null
        Start-Sleep -Seconds 6
        Contains 'StopMatch0' 'Newest manual stop'
        SetText 'NearbyScenario' 'nearby-slow'; Click 'FindNearbyStops'; Status 'NearbyStatus' 'werden geladen'
        SetText 'StopQuery' 'Cancel old nearby'
        Nearby 'new'
        Start-Sleep -Seconds 6
        Contains 'StopMatch0' 'Neue Umgebung'; Contains 'StopSearchMetadata' 'Nearby new'
        SetText 'NearbyScenario' 'slow'; Click 'FindNearbyStops'; Status 'NearbyStatus' 'wird ermittelt'
        SelectTab 'Verbindungen'; Start-Sleep -Seconds 6; Wait 'OriginText' | Out-Null
        Assert ($null -eq (Find 'MonitorStop')) 'Abandoned location never navigates to monitor'
        Click 'OpenStopSearch'; Nearby; Click 'StopMatch0'; Status 'MonitorStatus' 'manuell aktualisiert'
        Back; SelectTab 'Verbindungen'; Wait 'OriginText' | Out-Null
        Write-Output 'PASS fixture location endpoints, failure/revocation, latest wins, nearby list/map/monitor, unknown distance/position, keyboard and back navigation'
    } elseif ($LiveMaps) {
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
        MapClick 0.5 (MarkerY 51.46)
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
        Click 'MapStation0'; Status 'MonitorStatus' 'manuell aktualisiert'; Contains 'MonitorStop' 'onerror'
        Back; Status 'MapStatus' 'Basiskarte geladen\.'; Back; Wait 'StopQuery' | Out-Null
        SetText 'StopQuery' 'map-slow'; Click 'FindStops'; Wait 'StopMatch0' | Out-Null; Click 'ShowStopMap'
        Wait 'MapCanvas' | Out-Null; Back; Wait 'StopQuery' | Out-Null
        SetText 'StopQuery' 'Newest map'; Click 'FindStops'; Wait 'StopMatch0' | Out-Null; Click 'ShowStopMap'
        Status 'MapStatus' 'Basiskarte geladen\.'; Start-Sleep -Seconds 6; Click 'MapInformation'; Contains 'MapMetadata' 'Newest map'; Back; SelectTab 'Verbindungen'
        SelectEndpoint 'Origin' 'Essen'; SelectEndpoint 'Destination' 'Berlin'; Click 'SearchJourneys'; Click 'Journey0'
        Click 'ShowJourneyMap'; Status 'MapStatus' 'Basiskarte geladen\.'; Contains 'MapDataStatus' 'Teilweiser Verlauf'; Click 'MapInformation'; Contains 'MapSegment0' 'RE 1.*3 gelieferte Punkte'; Contains 'MapSegment1' 'Fußweg.*2 gelieferte Punkte'
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
        Back; Assert (((Wait 'StopQuery').GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern).Current.Value) -eq 'Gelsenkirchen Hbf') 'Live back preserves query'; SelectTab 'Verbindungen'; Wait 'OriginText' | Out-Null
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
        Back; SelectTab 'Verbindungen'; Wait 'OriginText' | Out-Null
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
} finally {
    if (!$app.HasExited) { Stop-Process -Id $app.Id }
    if ($Favorites) { $env:FLOWNRW_UI_TEST_FAVORITES = $previousFavoritePath }
    $env:FLOWNRW_UI_TEST_REFRESH_SETTINGS = $previousRefreshPath
}

