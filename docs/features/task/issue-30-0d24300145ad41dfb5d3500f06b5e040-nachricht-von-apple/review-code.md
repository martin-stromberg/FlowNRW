# Code-Review

## Ergebnis

**Status:** Keine Befunde

## Verifikationen (UIScene-Migration + Privacy Manifest + EnableIos)

- **`SceneDelegate.cs`:** `MauiUISceneDelegate` als Basisklasse, `[Register("SceneDelegate")]` deckt sich exakt mit `UISceneDelegateClassName` (`SceneDelegate`) in `Info.plist` — der ObjC-Name, den iOS per `NSClassFromString` instantiiert, stimmt. Override-Signatur `DidEnterBackground(UIScene)` ist die korrekte `UISceneDelegate`-Methode (nicht die `UIApplication`-Variante), `base`-Aufruf erfolgt zuerst. `IosBackgroundRefresh` liegt im selben Namespace `FlowNRW` (`internal static`) und ist ohne `using` erreichbar — gleiche Situation wie zuvor im `AppDelegate`.
- **`AppDelegate.cs`:** App-Level-`DidEnterBackground(UIApplication)` korrekt entfernt — unter `UIApplicationSceneManifest` wird der App-Delegate-Callback von iOS nicht mehr aufgerufen, der Scene-Callback uebernimmt. `IosBackgroundRefresh.Register()` bleibt in `FinishedLaunching` (wird im Scene-Lifecycle weiterhin aufgerufen; BGTaskScheduler-Registrierung muss vor Launch-Ende erfolgen).
- **`Info.plist`:** plistlib-parse sauber, alle 10 Keys einzigartig. `UIApplicationSceneManifest` exakt nach Migrationsmuster: `UIApplicationSupportsMultipleScenes=false`, `UISceneConfigurations` → `UIWindowSceneSessionRoleApplication` → Array mit `UISceneConfigurationName=__MAUI_DEFAULT_SCENE_CONFIGURATION__` (der Name, den `MauiUIApplicationDelegate.GetSceneConfiguration` als MAUI-Default erkennt) + `UISceneDelegateClassName=SceneDelegate`. `CFBundleLocalizations=[en,de]` valide. `Schedule()` ist idempotent (Cancel+Submit), einmaliger Scene-Callback unproblematisch.
- **`PrivacyInfo.xcprivacy`:** plistlib-parse sauber. `NSPrivacyTracking=false`, leere TrackingDomains, PreciseLocation/AppFunctionality als einziger CollectedDataType (deckt sich mit `NSLocationWhenInUseUsageDescription`), fuenf Required-Reason-Kategorien (UserDefaults CA92.1, FileTimestamp C617.1, SystemBootTime 35F9.1, DiskSpace 85F4.1, ActiveKeyboard 54BD.1) — Standard-MAUI-Set.
- **`FlowNRW.csproj`:** `BundleResource` korrekt auf iOS-TFM konditioniert (`GetTargetPlatformIdentifier == 'ios'`), `LogicalName=PrivacyInfo.xcprivacy` legt die Datei flach ins Bundle-Root (Apple-Anforderung). `ApplicationVersion 1→9` beabsichtigte Buildnummern-Bumps.
- **`scripts/iOS-Deployment.ps1`:** `EnableIos=true` steht in genau den vier rsp-Buildern, die `dotnet build/publish` für `net10.0-ios` ausfuehren (Z. 1013 `Invoke-Build`, 1171 `Invoke-Run`, 1267 `Invoke-DeviceViaSsh`, 1348 `Invoke-SimulatorViaSsh`). Notwendig, weil `net10.0-ios` im csproj nur bei macOS oder `EnableIos=true` in `TargetFrameworks` steht (csproj Z. 11). Es existieren **keine** rsp-Builder in Nicht-Build-Aktionen (`upload`, `list` erzeugen keine Response-Files); die Property beeinflusst ohnehin nur die TFM-Evaluierung und waere auch dort harmlos. Trailing-Slash-Normalisierung `DotNetRootRemoteDirectory` (Z. 103–106) laeuft nur unter Windows und nur bei gesetztem Wert; die Default-Setzung (Z. 82) endet bereits mit `/`, User-Werte werden nachgezogen — behebt real das `…dotnetpacks`-Kontatenationsproblem der Xamarin-Targets.

## Hinweise (kein Befund)

- `EnableIos=true` wird auch auf dem macOS-Pfad gesetzt, wo das TFM ohnehin enthalten ist — redundant, aber wirkungslos.
- Die Normalisierung behandelt keinen `\`-Endung im User-Pfad (Remote ist macOS, Unix-Pfad erwartet) — theoretischer Edge-Case, nicht relevant.
- In derselben Arbeitskopie liegen weitere, bereits in review-code.1–4 begutachtete Diffs (Toolchain-Check, Workflow, Doku) — nicht Teil dieses Reviews.

## Geprüfte Dateien

- `FlowNRW/Platforms/iOS/SceneDelegate.cs` (neu)
- `FlowNRW/Platforms/iOS/AppDelegate.cs`
- `FlowNRW/Platforms/iOS/Info.plist`
- `FlowNRW/Platforms/iOS/PrivacyInfo.xcprivacy` (neu)
- `FlowNRW/Platforms/iOS/IosBackgroundRefresh.cs` (Kontext/Signaturen)
- `FlowNRW/Platforms/iOS/Program.cs` (Kontext)
- `FlowNRW/FlowNRW.csproj`
- `scripts/iOS-Deployment.ps1` (nur `EnableIos`-Stellen + Trailing-Slash-Normalisierung)
