# Umsetzungsplan: Überflüssige Überschriften

## Übersicht

Auf drei Seiten werden statische In-Page-Überschriften entfernt, die den
Shell-Navigationsleistentitel (`Title`) identisch oder ähnlich wiederholen:
`MapPage` („Favoritenkarte" u. a., identisch — Ticket-Beispiel), `SearchPage`
(„Verbindungen" ≈ „Verbindung suchen") und `StopSearchPage` („Haltestellen" ≈
„Haltestelle suchen"). Betroffen ist ausschließlich die View-Schicht in `FlowNRW/`;
`FlowNRW.Core` bleibt unverändert.

## Designentscheidungen

| Komponente / Bereich | Gewählter Ansatz | Begründung |
|----------------------|-----------------|------------|
| `DeparturePage`-Stationsname (`MonitorStop`, 28 pt, gebunden an `model.Title`) | Beibehalten | Der Text ist dynamischer Dateninhalt (identifiziert die überwachte Haltestelle), kein statischer Ansichtstitel. `docs/design/acceptance.md` fordert für den Einzelmonitor explizit eine „klare große Überschrift". Außerdem ist `MonitorStop` Marker der Windows-UI-Testsuite (>20 Verwendungen); auf Windows existiert keine sichtbare Navigationsleisten-Titelzeile, sodass der Name dort die einzige Ortsangabe ist. |
| Abschnittsüberschriften in Karten/Sektionen („Haltestellenliste", „Wohin möchtest du fahren?", „Haltestelle finden", „Zeitpunkt", „Nächste Haltestellen", „Abfahrten automatisch laden", „Diagnose") | Beibehalten | Sie gliedern mehrteilige Seiten und wiederholen den Fenstertitel nicht. Die Ticket-Anforderung zielt auf doppelte Seitentitel, nicht auf Strukturüberschriften. |
| Untertitel (`TransitVisuals.Secondary`) auf `SearchPage`/`StopSearchPage` | Beibehalten | Beschreibender Fließtext, keine Überschrift; trägt die Seitenerklärung weiter, nachdem der große Titel entfällt. |

## Programmabläufe

### Seitenaufbau ohne redundanten Titel

1. Beim Konstruktor der betroffenen `ContentPage` wird `Title` wie bisher gesetzt
   (`MapPage`: aus `model.Title`, `SearchPage`/`StopSearchPage`: feste Strings).
2. Das erste `Children.Add` der bisherigen Titel-`Label`s entfällt; das Layout beginnt
   direkt mit dem nächsten Element (`MapPage`: Zustandskarte `mapState`,
   `SearchPage`: Untertitel + Eingabekarte, `StopSearchPage`: Untertitel + Suchkarte).
3. `TransitVisuals.Page(layout)` wendet `ApplyRoles` wie bisher auf die verbleibenden
   Elemente an.

Beteiligte Klassen/Komponenten: `MapPage`, `SearchPage`, `StopSearchPage`, `TransitVisuals`

## Neue Klassen

Keine.

## Änderungen an bestehenden Klassen

### `MapPage` (View)

- **Geänderte Methoden:** Konstruktor — die Zeile
  `layout.Children.Add(TransitVisuals.Text(model.Title, 28, true));` entfällt.

### `SearchPage` (View)

- **Geänderte Methoden:** Konstruktor — die Zeile
  `content.Children.Add(TransitVisuals.Text("Verbindungen", 32, true));` entfällt.

### `StopSearchPage` (View)

- **Geänderte Methoden:** Konstruktor — die Zeile
  `layout.Children.Add(TransitVisuals.Text("Haltestellen", 32, true));` entfällt.

## Datenbankmigrationen

Keine.

## Validierungsregeln

Keine.

## Konfigurationsänderungen

Keine.

## Seiteneffekte und Risiken

- **Windows-Ansicht:** Die entfernten Überschriften waren auf Windows die einzige große
  Titelanzeige dieser drei Seiten; die jeweilige Tab-/Navigationsbezeichnung bleibt
  sichtbar. Bewusste Vereinheitlichung zugunsten der iOS-Anforderung.
- **UI-Tests:** Kein Skript referenziert die entfernten Elemente (keine `AutomationId`);
  `MonitorStop` und alle anderen markertragenden Elemente bleiben unverändert.
- **Layout:** Auf `SearchPage`/`StopSearchPage` beginnt der Inhalt künftig mit dem
  grauen Untertitel — gewollt; `Spacing`/`Padding` bleiben unverändert.

## Umsetzungsreihenfolge

1. **Überschrift `MapPage` entfernen**
   - Voraussetzungen: Keine.
   - Beschreibung: `layout.Children.Add(TransitVisuals.Text(model.Title, 28, true));` in `FlowNRW/MapPage.cs` entfernen.

2. **Überschrift `SearchPage` entfernen**
   - Voraussetzungen: Keine.
   - Beschreibung: `content.Children.Add(TransitVisuals.Text("Verbindungen", 32, true));` in `FlowNRW/SearchPage.cs` entfernen.

3. **Überschrift `StopSearchPage` entfernen**
   - Voraussetzungen: Keine.
   - Beschreibung: `layout.Children.Add(TransitVisuals.Text("Haltestellen", 32, true));` in `FlowNRW/StopSearchPage.cs` entfernen.

4. **Bauen und Tests ausführen**
   - Voraussetzungen: Schritte 1–3.
   - Beschreibung: `dotnet test FlowNRW.Tests/FlowNRW.Tests.csproj` sowie Format-/Build-Prüfung (`dotnet build`, `dotnet format --verify-no-changes` für `FlowNRW.Core`/`FlowNRW.Tests`).

## Tests

### Neue Tests

Keine — die Änderung entfernt ausschließlich Deklarationen im View-Aufbau; es gibt
keine neue Logik und keine neue Benutzerinteraktion.

### Betroffene bestehende Tests

Keine.

### E2E-Tests (Pflicht)

Keine neue oder geänderte Benutzerinteraktion — kein neuer E2E-Test erforderlich.
Bestehende Windows-UI-Tests bleiben unverändert lauffähig, da kein referenziertes
`AutomationId`-Element entfernt wird.

| Test / Testklasse | Grund der Anpassung |
|-------------------|---------------------|
| — | Keine Anpassung nötig |

## Offene Punkte

Keine.
