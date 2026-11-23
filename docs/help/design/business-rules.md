← [Zurück zur Übersicht](index.md)

# Design — Fachliche Regeln

## Statusableitung einer Abfahrt

**Umsetzung:** `DeparturePresentation.Status` in FlowNRW.Core, dargestellt über `TransitVisuals.StatusChip`.

1. `Realtime.Cancelled == true` → `Cancelled` (sticht alle anderen Felder).
2. Kein `Realtime.ActualTime` → `Scheduled` („Keine Echtzeitdaten").
3. `ActualTime ≠ PlannedTime` oder `Realtime.Delay > 0` → `Delayed` (Chiptext über `DelayText`: `"+n Min."`).
4. Sonst → `OnTime` (kein Chip).

Ein positiver Verspätungstext wird nie für Ausfälle oder ohne reale Abweichung erzeugt.

## Informationsdarstellung

- **Farbe ist nie alleiniger Informationsträger:** Jeder Status trägt Symbol plus Text (`StatusChip`); Ausfälle und Verspätungen sind auch ohne Farbwahrnehmung erkennbar. Das Datenqualitäts-Warnsymbol trägt seine Bedeutung über Glyph-Form, gesprochene Beschreibung und den aufklappbaren Detailtext — nicht über die `Warning`-Färbung allein.
- **Datenqualitätshinweis nur auf Abruf:** `HasDataNotice` gilt, wenn das angezeigte Ergebnis `Warnings`, `IsFallback` oder `IsStale` trägt. Dann ersetzt ein `warning`-Symbol die frühere Hinweiszeile; die Provenienz ist erst nach ausdrücklichem Tipp sichtbar. Echte Fehler-, Leer- und „Letzter Stand"-Zustände bleiben dagegen sichtbare Statuszeilen.
- **Keine erfundenen Werte:** Es werden nur gelieferte Felder (`Cancelled`, `Delay`, `ActualTime`, `PlannedTime`, `Platform`) dargestellt; fehlende Echtzeit bleibt ausdrücklich „Keine Echtzeitdaten" und gilt nicht als Pünktlichkeit. Der Linienbestand der Favoriten enthält nur gelieferte bzw. zuletzt vollständig persistierte Linien; die Entfernung naher Haltestellen ist entweder Provider-Angabe oder gekennzeichnete Luftlinie — ohne beides „Entfernung unbekannt".
- **Linienbadges** bleiben Textbadges und nur bei gelieferter Kategorie farbig (`DeparturePresentation.BadgeColor` unverändert, siehe [Fahrplanauskunft-Regeln](../fahrplanauskunft/business-rules.md)).
- **Bedienziele** ≥44×44 logische Einheiten; Symbolschalter sind 48×48. Jeder Symbolschalter trägt eine gesprochene Beschreibung; Zustandswechsel (Stern gefüllt/umrissen, Chevron, Lupe/Häkchen) aktualisieren die Beschreibung im selben Aufruf.
- **Kontraste:** Die neuen Semantik-Dunkelvarianten sind rechnerisch gegen `CardDark`/`GroupDark` geprüft (≥4,5:1, Werte und Kontraste im Kommentarblock von `Colors.xaml`).
- **Keine starren Kartenhöhen;** Icon-Glyphen dürfen bei 150 % Textskalierung nicht clippen (Nachweis in der [Icon-Matrix](verification/icon-matrix-run.md)).

## Symbolik-Umfang (festgelegte Grenzen)

- Shell-Tabs tragen Symbole; Titel und Routen bleiben unverändert.
- `ToolbarItem` „Haltestellen" bleibt Text (Plattformkonvention).
- Kartenmarker der Leaflet-Karte und Linienbadges sind bewusst vom Icon-Umbau ausgenommen.

## Anzeigename und technische Kennungen

- **Namensbereinigung nur bei wörtlicher Wiederholung:** `JourneyPresentation.DisplayName` entfernt den Teil vor dem ersten Komma nur, wenn der Rest mit genau diesem Text beginnt **und** an einer Wortgrenze endet (Leerzeichen, `(`, `-`, `/`; ordinal, groß-/kleinschreibungssensitiv). „Münster (Westf), Münster (Westf) Hbf" → „Münster (Westf) Hbf"; „Bochum, Hbf" und „Bergheim, Bergheimer Str." bleiben unverändert.
- **Keine technischen Kennungen in Trefferlisten:** `Source`, `Id`, `Dhid` und rohe Koordinatenzeilen werden in Haltestellen-, Nah- und Verbindungstreffern nicht mehr angezeigt. Koordinaten bleiben einzig in der „Ausgewählt"-Zeile, weil sie dort die Nutzereingabe selbst sind.
- **Identität unberührt:** `Stop.Name`, `Address`, gespeicherte Favoriten, Cache-Schlüssel und Providerzuordnung bleiben unverändert — die Bereinigung ist reine Anzeigeformatierung ohne Store-Migration.
