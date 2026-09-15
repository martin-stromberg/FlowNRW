# Projektplanprüfung – iOS ÖPNV-App

## Ergebnis

**Status:** Projektplan vollständig

Unabhängige erneute Prüfung am 2026-09-08 gegen issue.md, die vollständige aktuelle requirement.md, inventory.md und die verbindlichen Nutzerergänzungen. Geprüft wurde der aktuelle Vier-Schritte-Plan einschließlich sämtlicher Rahmenbedingungen, Akzeptanzkriterien und offener Punkte. Die einzige Lücke des archivierten Berichts project-plan-check.1.md ist durch die ausdrückliche Nutzerentscheidung und deren konsistente Übernahme geschlossen.

## Abgleich Anforderung ↔ Entwicklungsschritte

| Anforderung | Geplante Abdeckung | Bewertung |
|---|---|---|
| A01 / K01 – iOS, C#, MVVM, Visual Studio, Vorlage ersetzen | Schritt 2 Kundenanforderung/AK1; Schritt 4 AK7–8 | Vollständig |
| A02 / K02 – Start/Ziel als Adresse, Haltestelle, Koordinate; nächste Verbindungen | Schritt 1 AK1–2; Schritt 2 AK2–3 | Vollständig |
| A03 / K02 – Umstiege, Fußwege, Linien, Betreiber, Echtzeit | Schritt 1 AK2; Schritt 2 AK3 | Vollständig |
| A04 / K03 – regionale Priorität bei NRW-Start/Ziel | Schritt 1 Rahmenbedingungen/AK3; Schritt 2 AK4; Vorgehensentscheidung 4 | Vollständig; mindestens ein Punkt in NRW, keine Gleichsetzung mit VRR/Rechteck |
| A05 / K04 – Monitorwahl über Suche/Karte; Verspätung, Ausfall, Gleis/Steig | Schritt 1 AK2–3; Schritt 3 AK1–3; Schritt 4 AK1 | Vollständig |
| A06 / K05 – konfigurierbare Intervalle, Favoritenmonitore nach Entfernung | Schritt 3 AK4–5 | Vollständig, einschließlich Neustart/fehlendem Standort |
| A07 / K06 – bundesweite Suche und GPS-Nahbereich | Schritt 1 AK1; Schritt 2 AK2; Schritt 3 AK1 | Vollständig |
| A08 / K06 – Karte mit Haltestellen/Linienverläufen | Schritt 4 AK1–2 | Vollständig, gelieferte Geometrie und zugängliche Listenalternative |
| A09 / K03 – regionale Echtzeit ergänzen und Soll/Ist konsolidieren | Schritt 1 AK3; Schritt 2 AK4; Schritt 3 AK3 | Vollständig, eindeutige Zuordnung und unbekannte Werte |
| A10 / K03 – bundesweite API, NRW-EFA/TRIAS, Normalisierung, Fallback | Schritt 1 Rahmenbedingungen/AK1–4; Schritt 2 AK4; Schritt 3 AK3 | Vollständig; EFA als Ausprägung begründet |
| A11 / K07 – klare, barrierearme Gestaltung und Navigation nach Design | Schritt 2 AK8; Schritt 3 AK8; Schritt 4 AK1/5 | Vollständig |
| A12 / K01 – Views/ViewModels/Services, DI, asynchrone APIs | Schritt 1 Rahmenbedingungen; Schritt 2 Rahmenbedingungen/AK1; Schritte 3–4 Weiterverwendung | Vollständig |
| A13 / K08 – Cache, optimierte Abrufe, Fehler, Hintergrundaktualisierung | Schritt 1 AK4; Schritt 3 AK5–6; Schritt 4 AK3–4 | Vollständig, keine dauerhafte iOS-Intervallgarantie |
| A14 / K09 – keine ungewollte personenbezogene Speicherung, HTTPS, technische Daten | Schritt 1 AK4; Schritt 2 AK5; Schritt 3 AK6; Schritt 4 AK4 | Vollständig |
| A15 / K10 – Erweiterbarkeit Verbünde, Sharing, optionale Pushs | Schritt 1 AK6; Schritt 2 AK5; Schritt 4 AK8 | Vollständig ohne unbeauftragte aktuelle Integration |
| A16 / K11 – Service-/Modell-/UI-Tests, Fehler-/Performancelogs | Schritt 1 AK4–5; Schritt 2 AK6–7; Schritt 3 AK7–8; Schritt 4 AK6–8 | Vollständig, tatsächliche Windows-UI-Ausführung und Versuchsnachweise |
| A17 / K12 – Windows-Actions-Release/Tests erhalten; kein iOS-CI/Deployment; native iOS-Abnahme beim Nutzer | Rahmenbedingungen aller Schritte; Schritt 2 AK6–7; Schritt 3 AK7–8; Schritt 4 AK6–8; Vorgehensentscheidung 8 | Vollständig, keine lokale iOS-Ausführung als Abschlussblocker |

Die fünf fachlichen Abläufe Verbindungssuche, Abfahrten, Favoriten, Umgebung und Aktualisierung sind vollständig zugeordnet. Provider-/Geheimniskonfiguration, HTTPS, technische Speicherung, Cachegrenzen, Aktualisierungsintervalle und Kartenkonfiguration sind berücksichtigt. Konkrete technische Werte und Anbieterfeldprüfungen sind zulässige Lifecycle-Detailentscheidungen, keine ausgelassenen Kundenanforderungen.

Nicht-Ziele sind gewahrt: keine iOS-CI-/Deployment-Pipeline, kein neues automatisiertes Deployment, keine Veröffentlichung und keine unbeauftragten Zusatzprodukte aus dem Designarchiv. Windows-Release und Tests werden ausdrücklich erhalten. Die geänderte Abnahmestrategie entbindet nicht von iOS-Implementierung, Codeprüfung und manueller Prüfanleitung.

## Abhängigkeitsprüfung

Vier existierende Schritte: 1 ohne Voraussetzung; 2 nach 1; 3 nach 2; 4 nach 2 und 3. Beschreibung und Übersichtstabelle stimmen überein. Alle Verweise zeigen auf frühere Schritte; keine Zyklen. Schritt 4 erhält die Datenversorgung transitiv über Schritt 2. Die spätere Kartenwahl für Monitore wird in Schritt 3 ausdrücklich an Schritt 4 übergeben.

Jeder Schritt enthält eine eigenständig verständliche Kundenanforderung, betroffene Bereiche, Voraussetzungen, verbindliche Rahmenbedingungen und prüfbare Akzeptanzkriterien. Die Übergabe als Lifecycle-Eingabe ist vollständig. Schritt 1 liefert separat nutzbare fachliche Services und benötigt mangels UI-Änderung noch keine native UI-Abnahme. Schritte 2–4 enthalten die bestätigte Windows-/iOS-Prüfaufteilung jeweils selbst.

## Fehlende oder unvollständige Punkte

Keine.

Die frühere offene Abnahmeentscheidung ist ausdrücklich beantwortet: „Ja, Teste alle UI-Abläufe unter Windows soweit es möglich ist. iOS muss erst einmal bei mir liegen.“ Plan und Anforderung verlangen nun tatsächliche Windows-UI-Prüfungen aller Abläufe soweit technisch möglich, konkrete Versuchsnachweise bei Grenzen und eine manuelle iOS-Abnahmecheckliste für den Nutzer. Diese Übernahme ist konsistent; eine erneute Rückfrage ist nicht erforderlich.

## Hinweise

Der Status bestätigt Planvollständigkeit, keine bereits implementierte Funktion oder bestandene Produktabnahme. Die Datenanbieter bleiben technisch zu prüfen: Einzelproben begründen Entwicklungsoptionen, keine flächendeckende oder produktive Verfügbarkeitsgarantie. Schritt 1 fordert reale Adapter und Live-Nachweise; Fixtures dürfen eine fehlende echte Versorgung nicht verschleiern. Ein tatsächlich zwingender Zugangsmangel ist erst bei Nachweis als Blockade zu behandeln.

Die bisherige Bestandsaufnahme belegt fünf erfolgreiche Counter-Tests, keine native iOS-Ausführung. Der Plan behauptet keinen vorhandenen iOS-Testnachweis. Nicht ausführbare Windows-Flüsse dürfen erst nach ernsthaften Versuchen mit konkreten Ursachen dokumentiert werden; reine ViewModel-Tests oder pauschale Plattformhinweise erfüllen die UI-Prüfanforderung nicht. Native iOS-Abnahme bleibt gemäß ausdrücklicher Nutzerentscheidung beim Nutzer und darf keinen erneuten lokalen Abschlussstopp auslösen.
