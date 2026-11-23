# Gezielte visuelle Nachprüfung am 03.10.2026

Prüfer: Projektkoordinator (Root), getrennt vom implementierenden Lifecycle-Agenten. Keine Produktänderungen durch diesen Prüfer. Dies ist eine gezielte Nachprüfung der zuvor separat festgestellten Befunde, keine neue vollständige Blindprüfung und keine funktionale Projektabnahme.

Die folgenden tatsächlichen PNGs unter `revised-matrix/` wurden mit `view_image` geöffnet. Die jeweilige Buildzuordnung steht in `revised-matrix/matrix.jsonl`; die Dateien müssen bei späteren Änderungen erneut zugeordnet werden.

| Gegenstand | Tatsächlich betrachtete Bilder | Ergebnis |
|---|---|---|
| V1 Suchformular | `light-1024-768-100-success-search-form.png`, `dark-430-900-150-success-search-form.png` | Beide Endpunktkarten und der primäre Suchbutton vollständig sichtbar. Keine unnötigen leeren Trefferflächen. Bei 150 Prozent umbrechen die Standortbuttons zwischen Wörtern. Der deaktivierte Suchbutton entspricht den leeren Endpunkten. |
| V2 Trefferhierarchie | Jeweils `light-1024-768-100-success-` und `dark-430-900-150-success-` mit `stop-list.png` und `map-native-list.png` | Stationsnamen stehen als größere auswählbare Aktionen im Vordergrund; Kennungen und Koordinaten sind kleinere sekundäre Zeilen. In der Kartenliste sind beide Treffer vollständig lesbar. Die Haltestellensuche bleibt scrollbar. |
| Mehrere Favoriten | `light-1024-768-100-success-home-distance-known.png`, `...-known-second.png`, `...-known-third.png` | Scrollfolge belegt Near mit 111 m, Far mit 4938 m und Missing mit unbekannter Entfernung. Abfahrtsdaten und Ausfalltext sind sichtbar; fehlende Entfernung wird nicht erfunden. |
| Kartenwahl | `light-1024-768-100-success-map-selected-station.png` | Zugeordneter Monitor Essen Hauptbahnhof mit Abfahrtsdaten sichtbar. Der funktionale Auswahlweg benötigt zusätzlich den nativen Testnachweis. |
| Vollständiger Fußweg und Folgeabschnitt | `dark-430-900-150-success-journey-walk-distance.png`, `...-journey-following-leg.png`, `...-journey-transfer-summary.png` | Fußweg 300 m / 5 Minuten vollständig sichtbar, danach Bus 10, Betreiber Fixture Bus und Umstieg 10 Minuten in nachvollziehbarer Scrollfolge. Fußwegbadge bleibt ungebrochen. Fehlende Echtzeit und fehlender Kartenverlauf werden ausdrücklich bezeichnet. |

Die hier geprüften alten V1-/V2-Befunde und genannten Bildlücken sind in diesen Aufnahmen behoben. Die Bilder zeigen das native Appfenster und keine leeren Bitmaps oder fremden Desktopinhalte. Weitere Varianten, Kontrastmessungen, tatsächliche Bediengrößen, native Funktionsregressionen und iOS bleiben Gegenstand der zugehörigen separaten Nachweise. Kein Gesamtabschluss allein aus diesem Bericht.
