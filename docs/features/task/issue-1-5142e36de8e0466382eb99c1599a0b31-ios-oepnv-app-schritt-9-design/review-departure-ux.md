# Planreview – Verdichtete mobile Abfahrtsanzeige

**Nachprüfung:** 04.10.2026
**Grundlage:** `requirement-departure-ux.md`, `plan-departure-ux.md` und aktueller Arbeitsbaum
**Status:** Die zuvor gemeldeten Codeblocker sind korrigiert. Paket 7 hat jetzt einen relevanten, aber unvollständigen UI-Testentwurf; die Gesamtanforderung bleibt offen.

## Prüfung der zuvor gemeldeten Befunde

- Fehlende oder falsch typisierte Nutzdaten werden jetzt in den EFA- und DB-REST-Mappern als `invalid_response` behandelt. Ein explizites leeres Array bleibt ein valides leeres Ergebnis.
- Der Provider-Orchestrator nimmt nun bis zu `maxResults + 1` zusammengeführte Ereignisse entgegen, erkennt den Überhang und markiert die Antwort mit `truncated-response`. Die UI-/Cache-Vollständigkeitsregel lehnt solche Antworten ab.
- Die Standortstatusmeldung unterscheidet nun Favoriten ohne Haltestellenkoordinaten und beginnt in diesem Fall mit einem sichtbaren Hinweis („Standort ermittelt … fehlen Koordinaten“). Erfolgreiche Positionen werden sowohl aus der Nahbereichssuche als auch aus der manuellen Entfernungsaktion an die Favoritenprojektion weitergegeben.

## Abgleich der Pakete

- **Paket 1 – Standort/Entfernung:** Implementierung der Positionsweitergabe und Diagnose für fehlende Haltestellenkoordinaten vorhanden. Die im Plan geforderten UI-Fixtures für Berechtigungsfehler, deaktivierte Dienste und Positionsfehler sind im Diff nicht enthalten.
- **Paket 2 – Kartenverdichtung:** Im Code umgesetzt: Ist-Zeit prominent, abweichende Soll-Zeit darunter durchgestrichen; redundante Texte und Quellenzeilen entfernt; geplantes Gleis nur bei Änderung.
- **Paket 3 – Ladeindikator:** Indikatoren sind auf Favoritenkarte und Detailkopf vorhanden, Statusmeldungen werden während des Abrufs ausgeblendet. Verzögerte UI-Abnahme für Erfolg, Fehler und Abbruch fehlt.
- **Paket 4 – Detailübernahme:** Vorhandene zukünftige Abfahrten werden beim Öffnen übergeben und während des Abrufs gehalten. Vorgeschriebene UI-Szenarien für Erfolg, Fehler und Abbruch fehlen.
- **Paket 5 – Linieninventar:** Additive Cacheerweiterung und Vollständigkeitsbehandlung umgesetzt. Neue Core-Abdeckung für fehlende Nutzdaten und Orchestrator-Kürzung ist vorhanden; JSON-Migrations- und vollständige Cacheintegrationstests sind im Diff weiterhin nicht erkennbar.
- **Paket 6 – Eingeklappte Karten:** Standardmäßig eingeklappt, Linien mit jeweils nächster bekannter Zeit werden dargestellt, und erfolgreiche vollständige Antworten aktualisieren das gemeinsame Inventar. Persistenz-/Neustart- und Expand/Collapse-UI-Nachweis fehlt.
- **Paket 7 – Akzeptanzprüfung: teilweise umgesetzt, nicht abgenommen.** `WindowsDepartureCacheUiTests.ps1` prüft nun Startseiten-Cache, verzögerten Startabruf, erfolgreiche Detailübernahme, sichtbaren Ladeindikator, Home-Fehler, Neustart, Warnungsantwort sowie vollständige Linienersetzung und leeres Ergebnis. Die Nachweise sind derzeit nur im Skript formuliert: der aktuelle Prozesslauf ist nicht protokolliert. Zudem fehlen explizit abgelaufene Abfahrten mit Linien ohne Zeit nach Neustart, Detailfehler/-abbruch, kontrollierter Lifecycle-Abbruch, Verifikation der leeren Antwort nach erneutem Start, strikte Prüfung dass alte Linien bei B/C-Ersetzung verschwinden, mehrere Linien nebeneinander ohne Zeit und die weiteren separaten Standort-/Refresh-/Lifecycle-Journeys aus Paket 7. `WindowsDesignUiTests.ps1` erstellt jetzt Screenshots für expandierte/eingeklappte Karte und prüft den Koordinatenhinweis; der geforderte vollständige Light/Dark-/Textgrößen-Nachweis ist damit noch nicht dokumentiert.

## Ergebnis

Die früheren Datenintegritäts- und Standortdiagnosebefunde sind geschlossen, und das neue E2E-Skript deckt zentrale Abläufe sinnvoll ab. Es reicht noch nicht für die Paket-7-Abnahme: einige ausdrücklich geforderte Fehler-/Abbruch-/Neustartfälle fehlen, einzelne Assertions erlauben alte Linien neben B/C beziehungsweise nach leerer Antwort, und für die geänderte Fassung liegt kein erfolgreicher nativer Prozessnachweis vor. In dieser Review-Nachprüfung wurden keine Tests ausgeführt.
