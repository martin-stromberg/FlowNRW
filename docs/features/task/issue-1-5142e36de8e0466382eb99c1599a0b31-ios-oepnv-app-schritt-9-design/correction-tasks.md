# Korrekturaufgaben aus dem Testfeedback

Erfasst: 03.10.2026

Die Rückmeldungen gehören zur laufenden visuellen Integration. Sie erweitern keine fachlichen Providerfunktionen und ändern die Vorgaben zu Windows-Tests, iOS-Abnahme und CI nicht.

| ID | Korrekturaufgabe | Abnahmekriterium |
|---|---|---|
| K1 | Stationsaktionen auf der Startseite als kompakte, beschriftete Symbolaktionen für Aktualisieren, Monitor öffnen und Favorit entfernen darstellen. Quellen-/Provenienztext platzsparend sekundär oder aufklappbar anbieten. | Die nächste Abfahrt bleibt im sichtbaren Kartenbereich; jede Aktion ist mindestens 44×44 logisch erreichbar, hat ein sichtbares Label/AccessibleName und die vollständige Quelle bleibt erreichbar. |
| K2 | Nahe Haltestellen zusätzlich zu gespeicherten Favoriten auf der Startseite anzeigen, wenn weitere Stationen im gültigen Radius liegen. | Mit synthetischem Standort und mindestens einem Favoriten plus weiteren Nearby-Stationen werden beide Gruppen getrennt und ohne Duplikate angezeigt; unbekannte Entfernung bleibt ehrlich. |
| K3 | Unverständliche doppelte Favoriten-/Stationszähler entfernen oder verständlich beschriften. | Die Startseite verwendet eindeutige Texte wie „3 Favoriten gespeichert“ bzw. „Nächste Haltestellen“; kein doppelter identischer Zähler bleibt sichtbar. |
| K4 | Bei Fokus auf Start-/Zielfeld Favoriten als Auswahlvorschläge einblenden und während der Eingabe filtern. | Fokus öffnet eine zugängliche Favoritenliste; Texteingabe filtert Namen/Adresse ohne Provideraufruf; leere Treffer und Auswahl sind verständlich. |
| K5 | Nach Auswahl eines Suchtreffers die Ergebnisliste ausblenden und den Haltestellennamen im Eingabefeld anzeigen. Technische Identität bleibt intern bzw. sekundär zugänglich. | Auswahl schließt die Trefferliste, das Feld zeigt den Namen, Routing nutzt weiterhin die vollständige Identität, und Zurücksetzen ermöglicht eine neue Suche. |
| K6 | Suchkarten verkleinern: Suche und Standort als kompakte, beschriftete Symbolaktionen horizontal neben dem jeweiligen Eingabefeld anordnen. | Start und Ziel bleiben auf schmalen und breiten Fenstern lesbar; Aktionen sind mindestens 44×44, beschriftet/accessible und verlieren keine bestehende Standortfunktion. |
| K7 | Verbindungszeitpunkt ergänzen: standardmäßig „Jetzt“, alternative Zeit auswählbar und Umschaltung zwischen Abfahrt und Ankunft. | Neue Suche startet mit „Jetzt“ und „Abfahrt“; alternative lokale Zeit sowie „Ankunft“ werden im Request-Modell weitergegeben und bleiben bei Validierung/Zurücknavigation erhalten. |

## Nicht-Ziele

- Keine neue Providerlogik, keine aktive Reisebegleitung, kein Tracking, keine Pushfunktion.
- Windows bleibt Release-/Testplattform in GitHub Actions; iOS-CI und automatisches Deployment bleiben unverändert.
- iOS-Geräteabnahme bleibt beim Nutzer.

## Reihenfolge

1. Startseite: K1–K3 einschließlich Nearby-Projektion und verständlicher Statuszeilen.
2. Suche: K4–K6 einschließlich kompakter Eingabekarten und Auswahlzustand.
3. Routingzeit: K7 im bestehenden Suchmodell und den Providerparametern.
4. Native Windows-Regressions- und Designmatrix, Code-/Usability-Review und Aktualisierung der Abnahmedokumentation.

## Bearbeitungsstand

- K1 umgesetzt: Stationsaktionen sind als kompakte Symbolaktionen mit zugänglichen Beschreibungen umgesetzt; Provenienz ist auf zwei sichtbare Zeilen begrenzt und vollständig semantisch erreichbar.
- K2 umgesetzt: Die Startseite lädt zusätzliche nahe Haltestellen und trennt sie von gespeicherten Favoriten.
- K3 umgesetzt: Favoritenzähler und Speicherstatus verwenden verständliche Formulierungen ohne doppelten Zähler.
- K4–K6 umgesetzt: Favoritenvorschläge bei Fokus, Filterung, Übernahme in das Feld, Ausblenden der Trefferliste und kompakte Eingabeaktionen sind umgesetzt.
- K7 umgesetzt: Zeitpunkt „Jetzt“, alternatives Datum/Zeit und Ankunftsmodus sind im Suchmodell und in der Eingabeseite ergänzt.

Der Windows-UiTest-Build wurde in der Entwicklerumgebung erfolgreich ausgeführt. Die native Designmatrix lief vollständig mit PASS durch; Core- und Testprojekt bauen mit 0 Warnungen/Fehlern, 255 Tests bestehen.
