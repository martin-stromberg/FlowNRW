# Projektplanprüfung – iOS ÖPNV-App

## Ergebnis

**Status:** Projektplan lückenhaft

Geprüft am 2026-09-07: aktueller Vier-Schritte-Plan gegen vollständige requirement.md (A01–A17, K01–K12), issue.md, inventory.md und die verbindliche Nutzerergänzung zum Erhalt der Windows-Release-/Testabläufe. Der fachliche Implementierungsumfang ist abgedeckt. Die ausdrücklich gestellte Frage zur nativen Abnahmestrategie bleibt jedoch unbeantwortet; damit ist der Plan noch nicht abschließend geklärt und dieser Bericht erteilt keine Implementierungsfreigabe.

## Abgleich Anforderung ↔ Entwicklungsschritte

| Anforderung | Entwicklungsschritte / Nachweis im Plan | Bewertung |
|---|---|---|
| A01 / K01 – iOS, C#, MVVM, Vorlage ersetzen, Visual Studio | 2: Kundenanforderung und AK1; 4: integrierte Plattformprüfung/Dokumentation | Geplant |
| A02 / K02 – Adressen, Haltestellen, Koordinaten, nächste Verbindungen | 1 AK1–2; 2 AK2–3 | Geplant |
| A03 / K02 – Umstiege, Fußwege, Linien, Betreiber, Echtzeit | 1 AK2; 2 AK3 | Geplant |
| A04 / K03 – NRW-Priorität | 1 Rahmenbedingungen/AK3; 2 AK4 | Geplant, Gebietsprüfung ausdrücklich mehr als VRR/Rechteck |
| A05 / K04 – Auswahl über Suche/Karte, Abfahrtszustände, Gleis | 1 AK2–3; 3 AK1–3; 4 AK1 | Geplant |
| A06 / K05 – Intervalle, Favoritenmonitore, Entfernung | 3 Kundenanforderung/AK4–5 | Geplant |
| A07 / K06 – bundesweite Suche, GPS-Nahbereich | 1 AK1; 2 AK2; 3 AK1 | Geplant |
| A08 / K06 – Haltestellenkarte, Linienverläufe | 4 AK1–2 | Geplant, Geometrie nur soweit geliefert |
| A09 / K03 – regionale Echtzeit ergänzen/konsolidieren | 1 AK3; 2 AK4; 3 AK3 | Geplant |
| A10 / K03 – bundesweiter Anbieter, EFA/TRIAS, Normalisierung, Fallback | 1 Rahmenbedingungen/AK1–4; 2 AK4; 3 AK3 | Geplant; EFA als gewähltes Protokoll ist begründet |
| A11 / K07 – Design, klare Navigation, Barrierearmut | 2 AK8; 3 Rahmenbedingungen; 4 AK1/5 | Geplant |
| A12 / K01 – Views/ViewModels/Services, DI, async | 1 Rahmenbedingungen; 2 Rahmenbedingungen/AK1; 3/4 Weiterverwendung | Geplant |
| A13 / K08 – Cache, Netzoptimierung, Ausfälle, Hintergrund | 1 AK4; 3 Intervalle/Fehler; 4 AK3–4 | Geplant |
| A14 / K09 – Datenschutz, HTTPS, technische Persistenz | 1 AK4; 2 AK5; 3 Favoriten/Datenschutz; 4 AK4 | Geplant |
| A15 / K10 – Erweiterbarkeit Verbünde, Sharing, optionale Pushs | 1 AK6; 2 AK5; 4 AK8 | Geplant ohne unbeauftragte Produktintegration |
| A16 / K11 – Service-/Modell-/UI-Tests, Fehler-/Performancelogs | 1 AK4–5; 2 AK6–7; 3 Tests; 4 AK6–7 | Inhaltlich geplant; verbindliche native Abnahmeaufteilung noch offen |
| A17 / K12 – Windows-Actions-Release und Tests erhalten, kein iOS-Deployment | Rahmenbedingungen sämtlicher Schritte; 2 AK7; 4 AK7–8 | Geplant, Nutzerergänzung korrekt erhalten |

Konfiguration ist abgedeckt: austauschbare Provider/Geheimnisse, HTTPS, Cachegrenzen, Aktualisierungsintervalle und sichere Grenzen, technische Favoritenhaltung, Kartenkonfiguration. Konkrete technische Werte dürfen gemäß Auftrag im Lifecycle begründet werden. Keine neue Nutzerentscheidung über bereits technisch prüfbare Entwicklungszugänge wird verlangt.

Nicht-Ziele sind gewahrt: keine iOS-CI-/Deployment-Pipeline, kein neues automatisiertes Deployment, keine Veröffentlichung; Design-Zusatzfunktionen werden nicht unbemerkt umgesetzt. Die fünf fachlichen Abläufe aus requirement.md sind über Datenversorgung, Verbindungssuche, Monitor/Favoriten, Umgebungskarte und Aktualisierung abgedeckt.

## Abhängigkeitsprüfung

Vier existierende Schritte, konsistente Übersichtstabelle und Beschreibungen: 1 ohne Voraussetzung, 2 nach 1, 3 nach 2, 4 nach 2 und 3. Alle Verweise zeigen auf frühere Schritte; keine Zyklen. Transitive Abhängigkeit von 4 auf die Datenversorgung ist über 2 vorhanden. Kartenwahl ergänzt Schritt 3 ausdrücklich in Schritt 4.

Die Schritte enthalten jeweils Kundenanforderung, Bereiche, Abhängigkeiten, Rahmenbedingungen und prüfbare Akzeptanzkriterien und eignen sich fachlich als Lifecycle-Eingaben. Schritt 1 ist eine separat testbare Service-Lieferung; seine Unabhängigkeit von nativer UI-Abnahme löst die offene Projektplanentscheidung nicht auf. Die Aussage unter „Vier Ausbaustufen“, jeder Schritt liefere bedienbare Funktionen, ist für Schritt 1 als Service-Nutzung zu verstehen; er liefert ausdrücklich noch keine fachliche UI.

## Fehlende oder unvollständige Punkte

- [ ] **Verbindliche Abnahmestrategie ungeklärt:** Unter „Offene Punkte“ steht korrekt, dass die bereits gestellte Nutzerfrage zur Windows-UI-E2E-Abnahme und späteren manuellen iOS-Abnahme bzw. einem verfügbaren Mac unbeantwortet ist. Vor einer vollständigen Planfreigabe muss die Antwort oder eine ausdrücklich autorisierte Annahme eingearbeitet werden. Danach müssen AK6–7 der UI-Schritte und die Abschlusskriterien eindeutig festlegen, welche iOS-Nachweise vor Projektabschluss erforderlich sind und welche gegebenenfalls als autorisierte spätere Prüfung verbleiben. Die Erhaltung der Windows-CI allein autorisiert kein Verschieben der iOS-Abnahme. Schweigen ist keine Zustimmung.

## Hinweise

Keine weitere fachliche Abdeckungslücke festgestellt. Unsichere Live-Verfügbarkeit wird korrekt nicht als Garantie dargestellt: VRR-Einzelproben begründen eine Entwicklungsoption, db-rest HTTP 503 verlangt weitere technische Prüfung. Lifecycle muss reale Nutzung und Grenzen nachweisen; fehlende Anbieterzugänge dürfen nicht mit Fixtures verdeckt werden.

Die Bestandsaufnahme nennt fünf erfolgreiche Counter-Tests, aber keine iOS-Ausführung; der Plan behauptet keine bestehenden nativen Testnachweise. Erhalt der Windows-Pipelines und Verzicht auf iOS-CI sind klar von lokaler Produktprüfung getrennt. Dieser Bericht prüft Planung, nicht implementierte Funktion oder erfolgreiche Abnahme.
