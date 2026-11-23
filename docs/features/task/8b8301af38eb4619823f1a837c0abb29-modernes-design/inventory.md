# Bestandsaufnahme: Modernes Design auf AAA-Niveau

Die Anforderung verlangt einen visuellen Qualitätssprung der .NET-MAUI-Oberfläche (konsequentes Designsystem, klarere Listeneinträge, echte Icons statt Unicode-Textglyphen, lizenzfreie Typografie). Diese Bestandsaufnahme dokumentiert den Ist-Zustand der betroffenen Oberflächen-, Style- und Testartefakte.

## Zusammenfassung

- **Vorhanden – Design-Tokens:** Vollständiges Farb-/Style-Token-System in `FlowNRW/Resources/Styles/Colors.xaml` und `Styles.xaml` mit `AppThemeBinding` (hell/dunkel), 44×44-Mindestzielen (`MinimumHeightRequest`/`MinimumWidthRequest`), Kartenradius 18 (`Border`-Style), `Button`-Radius 12 — abgestimmt auf `docs/design/acceptance.md` und `docs/design/reference/trans_nrw_mobil_system/DESIGN.md`.
- **Vorhanden – Zentrale Visual-Helfer:** `FlowNRW/TransitVisuals.cs` kapselt `Candidate` (Haltestellen-Listeneintrag), `Badge` (Linienbadge), `SecondaryAction` (48×48-Icon-Button mit Glyphen-Text), `Page`, `ApplyRoles`, `Text`, `Secondary`.
- **Vorhanden – Listeneinträge/Karten:** `DepartureCardView` (Abfahrtszeile), `JourneyCardView` (Ergebniskarte), `JourneyTimelineView` (Detail-Timeline), Favoritenkarten in `HomePage.RenderCards()`, Kandidatenlisten via `TransitVisuals.Candidate` in `HomePage.RenderNearby()`, `StopSearchPage`, `SearchPage.Endpoint` und `MapPage`.
- **Vorhanden – Lizenzfreie Fonts:** `OpenSans-Regular.ttf`/`OpenSans-Semibold.ttf` in `FlowNRW/Resources/Fonts/`, registriert in `MauiProgram.ConfigureFonts` (`MauiProgram.cs` Z. 28–32) als `OpenSansRegular`/`OpenSansSemibold`; eingebunden via `MauiFont Include="Resources\Fonts\*"` in `FlowNRW.csproj` Z. 67.
- **Fehlt – Echte Icons:** Sämtliche Aktions-/Statussymbole sind Unicode-Textglyphen in `Button.Text` (`＋ ⌖ ↻ ⚙ ⌃ ⌄ ▣ ☆ ★ ⌕ ⇄ ⓘ ✓`, siehe `inventory/icons.md`). Es gibt keine Icon-Font, keine `FontImageSource`-/`ImageSource`-Nutzung, keine SVG-Icons (`Resources/Images/` enthält nur `dotnet_bot.png`) und keine zentrale Icon-Definition (`TransitIcons` o. ä.).
- **Fehlt – Typografie-Entscheidung:** Registrierte Fonts werden nirgends als `FontFamily` referenziert (Styles und `TransitVisuals` setzen nur `FontSize`/`FontAttributes`). Einzige explizite Schriftwahl: `Consolas` (WinUI) bzw. `Menlo` für tabellarische Zeiten in `DepartureCardView.cs` Z. 37.
- **Stabil zu halten:** `AutomationId`s und `SemanticProperties.Description` werden von den UI-Test-Skripten (`tests/WindowsJourneyUiTests/*.ps1`) und der Barrierefreiheit genutzt; `#if UI_TEST_FIXTURES`-Szenariofelder sind in allen Seiten verdrahtet (`MauiProgram.cs` Z. 78–84, `App.xaml.cs` Z. 36–45).
- **Gebunden an Referenz:** `DeparturePresentation.BadgeColor`/`Describe` und `JourneyPresentation` in `FlowNRW.Core/Presentation/` liefern Badgefarben und zugängliche Beschreibungen; die Anforderung sieht keine Änderung an ViewModels/Datenlogik vor.

## Details

- [Design-Tokens, Styles und Fonts](inventory/styles.md) — `Colors.xaml`-Tokens, `Styles.xaml`-Styles, Schrift-/Bild-Assets, `MauiProgram.ConfigureFonts`, `FlowNRW.csproj`-Einbindung
- [Klassen und gebundene Eigenschaften](inventory/models.md) — View-Klassen, Pages und die ViewModel-Eigenschaften, an die sie binden
- [UI-Logik und Render-Methoden](inventory/logic.md) — `TransitVisuals`-Methoden, Render-/Aktualisierungsmethoden der Pages, `DeparturePresentation`/`JourneyPresentation`, Event-Abonnements
- [Glyph-Verwendung (Icon-Ersetzung)](inventory/icons.md) — Vollständige Tabelle aller Unicode-Glyphen mit Fundstelle, AutomationId und Beschreibung
- [Tests und UI-Test-Fixtures](inventory/tests.md) — Unit-Testklassen, UI-Test-Skripte, `UiTestFixtureServices`, relevante AutomationIds
