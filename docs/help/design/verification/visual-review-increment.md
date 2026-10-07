# Separater visueller Zwischenreview – Schritt 9

Stand: 02.10.2026. Prüfer: separater Agent `design_final_review`, ohne Beteiligung an der Implementierung. Gelesen wurden ausschließlich die Anforderung und `docs/design/acceptance.md`, nicht Implementierungsplan oder Code-Review. Keine Produktdateien verändert, keine GUI bedient und keine Builds gestartet. Lesen und Bildprüfung waren ohne Nutzungslimit-Fehler möglich.

**Ergebnis: sichtbarer Fortschritt, aber noch keine visuelle Freigabe.** Insbesondere die Kartenfläche, Beschriftungen bei 150 % und tatsächlich sichtbare Zustandsnachweise benötigen Nacharbeit. Dieser Bericht ersetzt weder den abschließenden separaten Matrixreview noch native Funktionsprüfungen.

## Grundlage und zeitliche Grenzen

Alle acht PNGs in `reference/` wurden mit `view_image` angesehen: Abfahrten, Verbindungssuche, Fahrtdetail und Umgebungskarte jeweils 430 × 900 sowie 1024 × 768. Dies sind laut Referenz-README korrekt gekennzeichnete lokale Renderings der Original-HTMLs. Ausgeschlossene Entwurfsfunktionen wie Auslastung, Tickets und aktive Reisebegleitung wurden nicht als fehlende Produktfunktionen gewertet.

Verglichen wurden tatsächliche native PNGs in `matrix/`, besonders die Serie `dark-430-900-150-success-*` vom 02.10.2026, beendet gegen 18:36 Uhr, sowie `light-430-900-100-success-*` und `dark-1024-768-100-success-*`. Die älteren 100-%-Bilder stammen teilweise vor der letzten Kompaktänderung; einzelne Homebilder sind neuer. Deshalb sind ihre Befunde Hinweise für die Wiederaufnahme und keine Behauptung über einen inzwischen geänderten Build. Das Manifest nennt DPI 96, Windows MAUI, App-Textskalierung und Commit `6780866c6b4e120704bfdf0160aa9a077768e06a` mit **uncommitted step 9**. Dieser Commit identifiziert die gezeigten Produktänderungen noch nicht eindeutig. Das appendierte Manifest enthält auch mehrfach verwendete Bildnamen; die abschließende Dokumentation muss die jeweils letzte Aufnahme eindeutig zuordnen.

## Konkrete Bildbefunde

| Bereich / Bild | Befund und erforderliche Nacharbeit |
|---|---|
| Home, `dark-430-900-150-success-home-distance-known.png`; aktuelles helles Gegenstück | Große Überschrift, klare Stationskarte, Linienbadges, tabellarisch wirkende Zeiten und textliche Verzögerung bilden eine erkennbare gemeinsame Gestaltung. Die kompakte Zeile ist deutlich näher am Entwurf als eine reine Textliste. Bei 150 % ist etwa eine vollständige Abfahrt sichtbar; das ist wegen Schriftgröße vertretbar, aber mehrere Favoriten und unbekannte Entfernung benötigen ergänzende Scrollbilder. |
| Monitor, `light-430-900-100-success-monitor-unknown-cancelled.png` | Unbekannte Echtzeit und Ausfall sind ausdrücklich lesbar; Farbe ist nicht alleiniger Träger. Soll/Ist und Bahnsteiginformation sind strukturiert. „Keine Echtzeit“ ist beim Ausfall durchgestrichen: lesbar, aber semantisch weniger klar als nur die konkrete ausfallende Fahrt/Zeit zu markieren. |
| Monitor, gleichnamiges dunkles 150-%-Bild | Zeigt verspätete, pünktliche und unbekannte Daten, jedoch nicht den ausgefallenen Eintrag im sichtbaren Bereich. Kein ausreichender Bildnachweis für Ausfall bei großer Schrift. |
| Suche, `dark-430-900-150-success-search-form.png` | Gruppierte Start-/Zielflächen sind erkennbar. „Übernehmen / suchen“ bricht mitten im Wort als „Übernehme / n / suchen“ um. Kürzere Beschriftung oder bei großer Schrift volle Breite verwenden. Die Suchaktion ist weit unterhalb des sichtbaren Startbereichs; ergänzendes Bild erforderlich. Auch die älteren breiten Bilder zeigen viel leere Kartenhöhe und nur Start/Ziel, während die Referenz Eingaben kompakt mit primärer Aktion zusammenfasst. |
| Ergebnisse, `dark-430-900-150-success-journey-results.png` | Zeiten, Dauer, Umstiege, Linien und Öffnen-Aktion sind klar gegliedert; die Karte funktioniert sichtbar auch mit großer Schrift. „1 Umstiege“ grammatisch korrigieren. Ausfall ist lesbar, könnte stärker dem betroffenen Abschnitt zugeordnet werden. |
| Details, `light-430-900-100-success-journey-details.png`, dunkles breites Gegenstück | Inhaltlich gegliederte Abschnittskarten mit Abfahrt/Ankunft und verständlichen unbekannten Werten. Gegenüber der Referenz weiterhin sehr viel gleichrangiger Text und keine klar sichtbare zusammenfassende Zeit-/Dauerübersicht. Die große Kartenaktion steht vor der Reiseübersicht. Hierarchie vor Abschluss erneut gegen den Entwurf prüfen. |
| Fußweg, `dark-430-900-150-success-journey-walk-transfer.png` | Badge wird zu „Fußw / eg“ umgebrochen. Breite erhöhen oder Badge oberhalb platzieren. Screenshot endet vor Gehstrecke/-dauer und dem anschließenden Abschnitt; ergänzende Aufnahme erforderlich, um den kompletten Umstieg nachvollziehen zu können. |
| Karte, `dark-430-900-150-success-map-stations.png`, `map-offline.png`, `map-no-position.png` | **Wesentlicher Gestaltungsfehler:** Text, Attribution und ausführliche Quellenwarnung verdrängen die Karte nahezu vollständig; sie beginnt erst bei ungefähr y=882 des 900 Pixel hohen Fensters. Die Referenz priorisiert dagegen die Karte. Kurze Statusinformation vor der Karte, Details kompakt zugänglich danach/an anderer Stelle darstellen. Alle fachlichen Warnungen müssen erhalten bleiben. „Haltestellenliste“ ist im Button rechts abgeschnitten. Bei 150 % Aktionen untereinander oder ausreichend umbrechbar anordnen. |
| Karte, helle schmale und dunkle breite 100-%-Bilder | Gleicher Hierarchiefehler in abgeschwächter Form: Karte beginnt erst ungefähr y=550 bzw. y=447. Native Liste ist zugänglich beschriftet, aber technische IDs/Koordinaten dominieren die Auswahltexte. Lesbarer Stationsname zuerst, Identitätsdetails sekundär. Synthetische Kartenkacheln sind ehrlich erkennbar und für deterministische Prüfung zulässig; sie beweisen nicht die visuelle Nutzbarkeit einer echten Basiskarte. |
| Fehlende Geometrie, `dark-430-900-150-success-journey-no-geometry.png` | Keine erfundene Linie; Einschränkung klar sichtbar. Auch hier fast keine Karte im Viewport. Text „Es wird kein Streckenverlauf erfunden“ ist eher Implementierungserklärung als Produkttext; „Für diese Verbindung ist kein Streckenverlauf verfügbar“ wäre verständlicher. |
| Einstellungen, `dark-430-900-150-success-settings-success-keyboard.png` | Auswahlkarte, Speichern und Erfolgsmeldung gut lesbar und ohne erkennbare Überdeckung. Das Bild beweist allein keinen Tastaturdurchlauf und keine tatsächliche Zielfläche des Pickers. Fehlerzustand fehlt. |
| Leere Favoriten und Monitorfehler, dunkle 150-%-Bilder | Leermeldung/Handlungsanweisung bzw. Fehler mit Aktualisieren sind sichtbar und verständlich. Keine sichtbaren Testszenario-Steuerfelder. Synthetische Namen wie „monitor-error“ sind Testdaten, keine ausgeblendeten fachlichen Fehler. |
| `dark-430-900-150-success-no-journeys.png`, `routing-provider-error.png`, `routing-loading.png`, `stop-list.png` | Dateinamen benennen den Zustand, aber die Bilder zeigen nur den oberen Formularbereich. Leer-/Fehler-/Lademeldung bzw. Trefferliste liegen außerhalb des Bildes. Für die Abnahme gezielt zur relevanten Meldung/Liste scrollen und deren Aktion mit aufnehmen. |

## Kontrast, Bedienung und Testgrenzen

Helle und dunkle Flächen, Haupttext, sekundärer Text und semantische Zustände wirken in den angesehenen Bildern grundsätzlich gut unterscheidbar. Dunkle Darstellung verwendet eigene Flächen und helle Akzente. Dies ist **keine gemessene Bestätigung** aller vorgeschriebenen Kontrastquotienten. Eine rechnerische Token-/Kontrastprüfung bleibt nötig, einschließlich deaktivierter Eingaben, Fokus und Badgevarianten.

Viele sichtbare Aktionsbuttons sind groß genug angelegt; aus PNGs kann die tatsächliche interaktive 44 × 44-Fläche nicht vollständig bestätigt werden. Insbesondere native Toolbaraktion „Haltestellen“, Zurück, Schalter und Karten-Zoom müssen anhand realer Bounds/Bedienung geprüft werden. Sichtbarer Fokus ist in einigen älteren Bildern vorhanden. 150-%-Apptext ist tatsächlich sichtbar größer; Tabs und Windowchrome bleiben kleiner. Das muss als App-Skalierungsnachweis, nicht als vollständiger Windows-/iOS-Systemtextnachweis dokumentiert werden.

Keine Testszenario-Eingabefelder in den angesehenen Aufnahmen. Keine privaten Standortdaten erkennbar. Die verbleibenden synthetischen Bezeichner/Kartenkacheln sind für diese Matrix erwartbar.

## Noch fehlende bzw. zu erneuernde Nachweise

- Aktuell sind drei Kombinationen vorhanden: hell/schmal/100 %, dunkel/breit/100 % und dunkel/schmal/150 %. Hell/breit sowie dunkel/schmal/100 % fehlen für eine eindeutige Home-Matrix beider Themen und Breiten. Ältere Aufnahmen nach den letzten Änderungen erneuern.
- Einstellungen mit Speicherfehler; tatsächlicher Tastatur- und Zielflächennachweis. Erfolg allein genügt nicht.
- Bilder mit wirklich sichtbaren Lade-, Leer- und Routingfehlerzuständen sowie manueller Wiederholung; sichtbare Haltestellentreffer und native Liste bei großer Schrift.
- Mehrere Favoriten einschließlich unbekannter Entfernung, Ausfall bei großer Schrift sowie vollständiger Fußweg/Umstieg durch ergänzende Scrollaufnahmen.
- Karte mit sichtbarer Kartengeometrie/Markern und Auswahl-/Monitorwirkung nach Korrektur der Fläche; Offline- und positionslose Zustände weiterhin erhalten.
- Abschließende Matrix eindeutig einem gespeicherten Produktstand zuordnen, danach separater finaler Bildreview und native Funktionsregression. iOS-Gerätebilder/VoiceOver/Dynamic Type bleiben beim Nutzer.

Die weitere Umsetzung darf fortgesetzt werden. Eine abschließende visuelle Abnahme oder ein Projektgesamtabschluss ist auf dieser Bildbasis noch nicht gerechtfertigt.
