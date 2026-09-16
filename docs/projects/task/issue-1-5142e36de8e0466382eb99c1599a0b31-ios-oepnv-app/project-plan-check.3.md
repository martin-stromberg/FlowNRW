# Projektplanprüfung – iOS ÖPNV-App

## Ergebnis

**Status:** Projektplan vollständig

Unabhängige Prüfung am 2026-09-15 des Neuschnitts auf acht Entwicklungsschritte. Geprüft wurden `issue.md`, `requirement.md`, `project-plan.md`, `package-review.md`, die bestehenden Branchzuordnungen in `steps.md` und der ursprüngliche Projektplan aus Commit `0d7eb4b`. Dies ist eine Planprüfung, keine Code- oder Schritt-Abnahme.

## Abgleich Anforderung ↔ Entwicklungsschritte

| Anforderung | Abdeckung im neuen Plan | Ergebnis |
|---|---|---|
| A01 – Vorlage ersetzen, iOS/C#/MVVM/Visual Studio | 2 AK 1 und Rahmen; 8 AK 5–6 | Vollständig |
| A02 – Adresse, Haltestelle, Koordinate und nächste Verbindungen | 1; 2 AK 2–3; GPS-Übernahme in 5 AK 1 | Vollständig |
| A03 – Umstiege, Fußwege, Linien, Betreiber, Echtzeit | 1 AK 2; 2 AK 3 | Vollständig |
| A04 – NRW-Priorität | 1 AK 3; 2 AK 4; gemeinsame Rahmen | Vollständig |
| A05 – Monitor über Suche/Karte, Abfahrtszustände | 3 AK 1–4; 4 AK 1; 5 AK 2 | Vollständig |
| A06 – Intervalle, favorisierte Startseitenmonitore, Entfernung | 6 AK 1–4; 7 AK 1–4 | Vollständig |
| A07 – Bundesweite Suche, GPS-nahe Haltestellen | 1; 3 AK 1; 5 AK 1–4 | Vollständig |
| A08 – Haltestellenkarte und Linienverläufe | 4 AK 1–4; 5 AK 2 | Vollständig |
| A09 – NRW-Echtzeit ergänzt bundesweite Soll-/Ist-Daten | 1 AK 3; 2 AK 4; 3 AK 3; Vorgehensentscheidung 4 | Vollständig |
| A10 – Nationale API, NRW-EFA, Normalisierung/Fallback | 1 AK 1–4; Rahmen und UI-Integration in 2–3 | Vollständig |
| A11 – Design, Navigation, Barrierearmut | Vorgehensentscheidung 2; Rahmen 2–8; 4 Listenalternative; 8 AK 3–4 | Vollständig |
| A12 – Views/ViewModels/Services, DI, asynchrone APIs | 1; Rahmen aller UI-Schritte; 2 Plattform-/UI-Basis | Vollständig |
| A13 – Cache, Netzlast, Ausfälle, Hintergrund | 1 AK 4; 3 AK 3; 7; 8 AK 1–2 | Vollständig |
| A14 – Datenschutz, HTTPS, technische Speicherung | 1 AK 4; Rahmen; 5 AK 3; 6 AK 1; 8 AK 2/6 | Vollständig |
| A15 – Weitere Verbünde, spätere Sharing-/Push-Erweiterung | 1 AK 6; Vorgehensentscheidung 7; 8 AK 6 | Vollständig |
| A16 – Modell-/Servicetests, UI-Tests, Diagnose | 1 AK 4–5; explizite Tests und Rahmen 2–8 | Vollständig |
| A17 – Windows-Test/Release erhalten, iOS-Abnahme beim Nutzer | Vorgehensentscheidung 8; Rahmen jeder Lieferung; 8 AK 5–6 | Vollständig |
| A18 – Autorisierte IIS-Zwischenstände | Vorgehensentscheidung 9; Rahmen 2–8; 2 AK 5; 8 AK 6 | Vollständig |
| A19 – Paketgrößen kritisch prüfen, Fortschritt/Branches erhalten | `package-review.md`; Vorgehensentscheidung 1; acht fachliche Lieferungen | Vollständig |

Die daraus abgeleiteten K01–K14 sind damit abgedeckt. Der Abgleich sämtlicher ursprünglicher Schritt-AK in `package-review.md` stimmt mit den tatsächlichen alten und neuen Schrittbeschreibungen überein: Alt 1.1–1.6 bleiben erhalten; alt 2.1–2.8 verteilen sich auf 2, 5 und gemeinsame Rahmen/Abschluss; alt 3.1–3.8 auf 3, 5, 6, 7 und Rahmen/Abschluss; alt 4.1–4.8 auf 4, 5, 8 und integrierte Tests. Keine ursprüngliche Anforderung entfällt durch spätere Lieferung innerhalb des Projekts.

Schritt 1 wurde zusätzlich zwischen beiden Plänen textuell verglichen: vollständiger Abschnitt einschließlich sechs AK ist nach Vereinheitlichung der Zeilenumbrüche und äußerem Whitespace wortgleich. Die offene fachliche Codekorrektur wird dadurch weder aufgehoben noch neu bewertet.

## Abhängigkeitsprüfung

| Schritt | Abhängigkeiten | Prüfung |
|---|---|---|
| 1 | Keine | Startpunkt |
| 2 | 1 | Vorhandener Datenkern |
| 3 | 2 | Bedienbare App und Navigation |
| 4 | 2, 3 | Details und Zielmonitor vorhanden |
| 5 | 2, 3, 4 | Manuelle Wege vor GPS-Erweiterung |
| 6 | 3, 5 | Monitore und Entfernungsvoraussetzung |
| 7 | 3, 6 | Einzel- und Favoritenmonitore |
| 8 | 2, 3, 4, 5, 6, 7 | Integrierter Lebenszyklusabschluss |

Alle Referenzen existieren, zeigen auf frühere Nummern und sind zyklenfrei; Übersichtstabelle und Schritttexte stimmen überein. Jede Lieferung besitzt Kundenanforderung, fachliche Bereiche, Abhängigkeiten, verbindliche Rahmen und konkrete AK einschließlich Prüfungen. Die Beschreibungen taugen als eigenständige Lifecycle-Eingaben unter Nutzung ihrer erklärten Vorgänger, ohne Zerlegung auf Klassenebene.

Die bestehenden Branchzuordnungen 1–4 sind im bisherigen Tracking erhalten. Der Plan verlangt ausdrücklich deren Bewahrung und erst danach Ergänzung von 5–8. Dass `steps.md` zum Prüfzeitpunkt noch den alten Viererschrittstand enthält, entspricht dem angekündigten anschließenden Trackingabgleich; es ist keine Planlücke.

## Fehlende oder unvollständige Punkte

Keine.

## Hinweise

Die Lieferungen sind gegenüber den bisherigen Sammelpaketen fachlich enger: manuelle Suche, Monitor, Karte, GPS, Favoriten, Intervalle und Lebenszyklus lassen sich getrennt begutachten. Schritt 2 umfasst weiterhin notwendige Plattform-/UI-Grundlage und vollständige Verbindungsdetails; die begründete gemeinsame Lieferung ist nachvollziehbar. Schritt 8 bündelt nur die verbleibende Lebenszyklusfunktion mit notwendiger integrierter Schlussprüfung.

Windows-UI-Ausführung soweit möglich, konkrete Nachweise technisch nicht ausführbarer Flüsse, iOS-Codeprüfung und manuelle Nutzerabnahme bleiben ausdrücklich verbindlich. Windows-CI und Releases werden erhalten; iOS-CI und neues automatisiertes Deployment bleiben Nicht-Ziele. IIS verteilt native Windows-ZIP-Pakete mit Startanleitung, Version/Commit, Umfang und Grenzen sowie Begutachtungsinformationen; der Plan fordert keine Browser-App. Prüfungen von Site/Zielpfad, Download und lokalem Start sind vorgesehen. Die externe Stakeholder-URL kann separat geklärt werden, ohne unabhängige Entwicklung zu blockieren.

Die Prüfung bestätigt die geplante Abdeckung, nicht bereits implementierte Funktionen, erfolgreiche native UI-Tests oder eine zukünftige Paketveröffentlichung. Bestehende Codeabnahme und deren Nachbesserung bleiben getrennte Verfahren.
