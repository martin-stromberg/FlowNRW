# Anforderung – Modernes Design auf AAA-Niveau

**Kundenanforderung (Original):** „wir brauchen für die App ein modernes Design auf Tripple A Niveau. Die Listeneinträge sollen vier besser rüberkommen. Es soll ein modernes minimalistisches Design geben, eine Vereinigung aus Ästhetik und Usability, weg vom Prototypen. Nutze vollbertige Symbole für die Aktionsbuttons und Status und verwende moderne lizenzfreie Typografie."

**Korrektur/Annahmen bei der Übersetzung:** „vier besser rüberkommen" wird als „viel besser rüberkommen" interpretiert; „vollbertige Symbole" als „vollwertige Symbole" (echte Iconografie statt Textglyphen); „Tripple A Niveau" als „AAA-Niveau" (hochwertige, professionelle visuelle Qualität).

## Fachliche Zusammenfassung

Die .NET-MAUI-Oberfläche von FlowNRW soll vom bisherigen, überwiegend prozedural in C# aufgebauten Prototyp-Erscheinungsbild zu einem modernen, minimalistischen Design mit professioneller visueller Qualität weiterentwickelt werden – bei gleichzeitiger Beibehaltung bzw. Verbesserung der Usability. Die Listendarstellungen (Abfahrten, Verbindungen, Haltestellen) sollen deutlich klarer strukturiert und lesbarer werden. Aktionsbuttons und Statusanzeigen sollen vollwertige Symbole (echte Icons) erhalten, die derzeit als Unicode-Textglyphen (`＋`, `⌖`, `↻`, `⚙`, `⌃`, `⌄`, `▣`, `☆`) in `Button.Text` realisiert sind. Zusätzlich soll eine moderne, lizenzfreie Typografie eingeführt werden (aktuell: OpenSans-Regular/-Semibold, registriert in `MauiProgram.ConfigureFonts`, sowie Systemschriften).

## Betroffene Klassen und Komponenten

### Zentrale Design-Ressourcen (FlowNRW)

- `FlowNRW/Resources/Styles/Colors.xaml` – Farb-Tokens (`Primary`, `Canvas*`, `Card*`, `Group*`, `Ink*`, `Muted*`, `Outline*`, `Gray*`)
- `FlowNRW/Resources/Styles/Styles.xaml` – implizite Styles für `Button`, `Border`, `Label`, `Entry`, `SearchBar`, `ImageButton`, `Switch`, `Picker` u. a.; Schlüssel-Styles `Headline`, `SubHeadline`
- `FlowNRW/App.xaml` – Merged Dictionaries der Style-Ressourcen
- `FlowNRW/TransitVisuals.cs` – zentrale Visual-Helfer: `SecondaryAction` (Icon-Buttons), `Badge` (Linienbadges), `Candidate` (Haltestellen-Listeneinträge), `Page`, `ApplyRoles`, `Text`, `Secondary`

### Listeneinträge / Karten (primärer Fokus „Listeneinträge")

- `FlowNRW/DepartureCardView.cs` – Abfahrtszeile als Karte (Linienbadge, Ziel, Soll-/Ist-Zeit, Ausfall, Gleis/Steig)
- `FlowNRW/JourneyCardView.cs` – Verbindungsergebnis-Karte
- `FlowNRW/JourneyTimelineView.cs` – Timeline im Verbindungsdetail
- `FlowNRW/HomePage.cs` – `RenderCards()`/`RenderNearby()`: Favoritenkarten, Aktionsbuttons (Auf-/Einklappen, Aktualisieren, Monitor öffnen, Favorit entfernen), Shortcut-Reihe
- `FlowNRW/StopSearchPage.cs`, `FlowNRW/SearchPage.cs`, `FlowNRW/ResultsPage.cs` – Kandidaten-/Ergebnislisten über `TransitVisuals.Candidate` bzw. eigene Layouts
- `FlowNRW/DeparturePage.cs` – Einzelmonitor-Liste
- `FlowNRW/MapPage.cs`, `FlowNRW/RefreshSettingsPage.cs`, `FlowNRW/JourneyDetailPage.cs` – sekundäre Oberflächen im selben Stil
- `FlowNRW/AppShell.xaml` / `AppShell.xaml.cs` – Shell-/Tab-Präsentation (Navigation, ggf. Tab-Icons)

### Assets und Registrierung

- `FlowNRW/Resources/Fonts/` – derzeit `OpenSans-Regular.ttf`, `OpenSans-Semibold.ttf`; ggf. neue lizenzfreie Schrift (z. B. Inter, SIL OFL) und/oder Icon-Font (z. B. Fluent UI System Icons, Material Symbols)
- `FlowNRW/Resources/Images/` – ggf. neue SVG-/PNG-Icon-Assets (`MauiImage`)
- `FlowNRW/MauiProgram.cs` – `ConfigureFonts`-Registrierung
- `FlowNRW/FlowNRW.csproj` – `MauiFont`-/`MauiImage`-Einbindung
- Neu: zentrale Icon-Definition (z. B. statische Klasse `TransitIcons`/`IconGlyphs` oder benannte Styles/`FontImageSource`-Ressourcen)

### Core / Präsentationslogik

- `FlowNRW.Core/Presentation/DeparturePresentation.cs` – `BadgeColor`, `Describe` (Status-Semantik für Listeneinträge)
- `FlowNRW.Core/Presentation/JourneyPresentation.cs` – `Address` (Kandidatenbeschreibung)
- Keine Änderung an ViewModels/Datenlogik erwartet; visuelle Umsetzung bleibt in der App-Schicht

### Tests

- `FlowNRW.Tests` – bestehende `PresentationTests`, `DeparturePresentationTests_Badges` etc.; ggf. neue Tests, falls Icon-/Statussemantik in `FlowNRW.Core.Presentation` verlagert wird
- UI-Test-Fixtures (`#if UI_TEST_FIXTURES`, AutomationIds) müssen stabil bleiben; AutomationIds in `HomePage`, `TransitVisuals.ApplyRoles` nicht umbenennen

## Implementierungsansatz

1. **Design-Tokens ausbauen, nicht neu erfinden:** Das vorhandene Token-System in `Colors.xaml`/`Styles.xaml` (Farben, 44×44-Touchziele, Radien, `AppThemeBinding` hell/dunkel) ist die verbindliche Basis; die Tokens entsprechen bereits `docs/design/acceptance.md` und `docs/design/reference/trans_nrw_mobil_system/DESIGN.md`. Verfeinerung über Abstandsskala (4/8/12/16/24/32-Raster), konsistente Radien, dezente Separatoren statt schwerer Rahmen, typografische Skala (Titel 28–34, Abschnitt 22, Text 17, Nebeninfo 15, Meta 13).
2. **Vollwertige Symbole:** Unicode-Glyphen in `TransitVisuals.SecondaryAction` und den Aktionsgrids in `HomePage`/`DeparturePage` durch echte Icons ersetzen – entweder als Icon-Font-Glyph (`FontImageSource`/Label mit registrierter `MauiFont`) oder als `MauiImage`-SVG in `ImageButton`. Zentrale, benannte Icon-Ressource einführen, damit Symbole konsistent und austauschbar sind. `SemanticProperties.Description` und AutomationIds beibehalten (Barrierefreiheit und Tests). Status (Verspätung, Ausfall, „keine Echtzeit", Lade-/Fehlerzustände) mit Symbol **plus** Text darstellen – Farbe bleibt nie alleiniger Informationsträger.
3. **Listeneinträge restrukturieren:** `DepartureCardView`, `JourneyCardView`, `TransitVisuals.Candidate` und die Favoritenkarten in `HomePage.RenderCards()` visuell verdichten: klare Zeilenstruktur (Badge, Ziel, Zeiten, Verspätung, Gleis/Steig), einheitliches Spacing, erkennbare Kartenhierarchie. Keine erfundenen Echtzeitwerte, Geometrien oder Zusatzdaten (verbindlich gemäß `docs/design/acceptance.md`).
4. **Typografie:** Moderne lizenzfreie Schrift (z. B. Inter unter SIL OFL; OpenSans wäre ebenfalls lizenzfrei, aber bereits Standard-Template-Schrift) in `Resources/Fonts/` ablegen, in `MauiProgram.ConfigureFonts` registrieren und `FontFamily` in `Styles.xaml`/`TransitVisuals` setzen. Tabellarische Zeitdarstellung (aktuell `Consolas`/`Menlo` in `DepartureCardView`) beibehalten oder auf tabulare Ziffern der neuen Schrift umstellen.
5. **Architektur bewahren:** MVVM, DI und die C#-basierte UI-Erzeugung bleiben unverändert; es entstehen keine neuen Seiten und keine Provider-/Datenänderungen. Helligkeits-/Dunkelheitsunterstützung über bestehende `AppThemeBinding`-/`SetAppThemeColor`-Mechanismen.

## Konfiguration

Keine neue Laufzeit- oder Anwendungskonfiguration erforderlich. Schrift- und Icon-Assets werden statisch über `FlowNRW.csproj` (`MauiFont`, `MauiImage`) ins Paket eingebunden; hell/dunkel bleibt über die vorhandenen Theme-Tokens gesteuert.

## Offene Fragen

1. **Icon-Bibliothek:** Soll eine Icon-Font (z. B. Fluent UI System Icons, Material Symbols – beide lizenzfrei) oder ein Satz SVG-Bild-Assets verwendet werden? Beide Varianten sind technisch möglich; Entscheidung beeinflusst `SecondaryAction`-Umbau und Theme-Färbung der Icons.
2. **Schriftwahl:** Konkrete Schrift festlegen (Inter vs. Beibehaltung/Ausbau von OpenSans vs. Systemschrift). Hinweis: `docs/design/acceptance.md` schreibt „Native Systemschrift mit derselben Hierarchie wie Inter/SF-Pro-Referenz; keine extern geladene Schrift erforderlich" – eine externe Schrift ist also zulässig, aber nicht vorgeschrieben; die neue Anforderung „moderne lizenzfreie Typografie" deutet auf eine explizite Schriftentscheidung.
3. **Verhältnis zu Schritt 9 / visueller Abnahme:** Die App wurde bereits gegen `docs/design/acceptance.md` visuell abgenommen (siehe `docs/features/task/issue-1-…-schritt-9-design/`). Ist diese Anforderung als Nachschärfung desselben Designsystems zu verstehen, oder ersetzt sie die bisherigen Token-/Abnahmeentscheidungen (z. B. Schriftvorgabe)?
4. **Begriff „AAA-Niveau":** Nicht objektiv messbar – reicht die bestehende Abnahmematrix aus `docs/design/acceptance.md` als Referenzrahmen, oder gibt es eine neue Designvorlage/Mockups?
5. **Umfang der Symbolik:** Nur Aktionsbuttons und Statusindikatoren, oder auch Linienbadges, Tab-/Navigationsicons und Kartenmarker?
6. **Plattformziel:** Gilt der Design-Refresh weiterhin primär für Windows/iOS wie im Projektstand, ohne neue Plattformanforderungen?
