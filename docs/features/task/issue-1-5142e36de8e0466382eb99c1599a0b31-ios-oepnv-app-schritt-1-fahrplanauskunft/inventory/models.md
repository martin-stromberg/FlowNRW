# Datenmodelle

Für die Schritt-1-Anforderung sind im bestehenden Fachkern keine ÖPNV-Datenmodelle vorhanden.

## `ClickCounter`

Datei: `FlowNRW.Core/ClickCounter.cs`

`ClickCounter` ist das einzige vorhandene Core-Modell bzw. zustandsbehaftete Fachobjekt. Es gehört zur MAUI-Standardvorlage und stellt keine Adresse, Haltestelle, Koordinate, Verbindung, Abfahrt oder Echtzeitinformation dar.

| Eigenschaft | Typ | Beschreibung / Zweck |
|-------------|-----|----------------------|
| `Count` | `int` | Anzahl der registrierten Klicks; privat setzbar. |
