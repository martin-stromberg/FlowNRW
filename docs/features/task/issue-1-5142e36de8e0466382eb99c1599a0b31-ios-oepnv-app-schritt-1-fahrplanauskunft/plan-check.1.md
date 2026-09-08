# Plan-Gegenprüfung

## Ergebnis

**Status:** Plan lückenhaft

## Abgleich Akzeptanzkriterien

| Akzeptanzkriterium | Umsetzung im Plan | Testnachweis im Plan | Status |
|--------------------|-------------------|----------------------|--------|
| AK1: Reale konfigurierbare bundesweite Suche/Routing, NRW-Abfahrten, Eingabearten/Nähe und Live-Proben | Beide Adapter mit Search/Nearby/Trip/Departures, Fachservices, EFA-Entwicklungsfallback und gesonderte Live-Proben; Umsetzung 4–8 | Mapper-/Fallbacktests, echte NRW-Abfahrt und bundesweite Verbindung einschließlich EFA bei db-rest-Ausfall | Abgedeckt |
| AK2: Vollständige gelieferte Verbindungs-/Abfahrtsfelder, unbekannte Werte, Zeitzonen/Tageswechsel | Modelle und beide Mapper, Umsetzung 2/4/5 | Konkrete Tests nennen ISO-8601, Soll/Ist, delay/platform und unbekannte Felder; Tageswechsel, EFA-Zeitzonen und Ausfälle fehlen als konkrete Prüfszenarien | Lücke |
| AK3: NRW-Priorität, bundesweiter Fallback, eindeutige Fahrt-/Haltestellenkonsolidierung, Warnungen/Metadaten | Grenzpolygon, Orchestrator und Konsolidierer, Umsetzung 1/6; DHID-Zuordnungsweg ist hinsichtlich Fahrtidentität nicht eindeutig beschrieben | NRW-/Fallback-/Mehrdeutigkeitstests vorhanden; gleiche Haltestelle mit unterschiedlichen Fahrten im DHID-Zweig muss ausdrücklich geprüft werden | Lücke |
| AK4: Begrenzter Cache, optimierte Abrufe, Cancellation/Retry, HTTPS und Datenschutz | HTTP-Gateway, In-Memory-Cache, Diagnose, konkrete Grenzen, Umsetzung 3/7 | TTL/Größe/Stale, Deduplizierung, Cancellation, Retry und Logtests; Tasks 38/39 ergänzen Timeout und Sicherheit | Abgedeckt |
| AK5: Deterministische Tests und Repository-Prüfungen, Windows-CI | Umsetzung 8/9 und Tasks 32–43, mindestens 70 % Core-Zeilenabdeckung | Bestehende fünf Counterfälle bleiben erhalten; Format, Warnungen als Fehler, Core-Coverage und Windows-Prüfungen geplant; fachliche Testlücken siehe AK2/3 | Lücke |
| AK6: Erweiterbare Grenzen, dauerhafte Dokumentation, begründete Werte und konkrete Zugangssperren | Interfaces/Optionsobjekte und Blockadestrategie vorhanden; dauerhaftes Dokument für zusätzliche Verbünde sowie spätere Sharing-/Push-Anbindung nicht als lieferbare Aufgabe enthalten | Providerproben und technische Dokumentation genannt; konkrete Dokumentationsprüfung der Erweiterungsgrenzen fehlt | Lücke |

## Fehlende oder unvollständige Testanforderungen

- [ ] AK2/5: Konkrete Fixtures und Assertions für Fahrt über Mitternacht, EFA-Zeitnormalisierung mit expliziter Zeitzone sowie gelieferte Ausfälle und den Unterschied zwischen fehlender Echtzeit und tatsächlich pünktlicher Echtzeit ergänzen. Die vorhandene allgemeine Mapper-/ISO-8601-Testbeschreibung benennt diese geforderten fachlichen Fälle nicht.
- [ ] AK3/5: Einen konkreten Cross-Provider-Test mit gleicher DHID/Haltestelle, aber unterschiedlichen Fahrten vorsehen. Nur die passende Fahrt darf Echtzeit erhalten; auch im DHID-Zweig darf Haltestellenidentität allein keinen Merge auslösen.
- [ ] AK6: Eine prüfbare Dokumentationsabnahme vorsehen, die das dauerhafte Dokument zu Provider-/Service-Erweiterungsgrenzen und späterer Sharing-/Push-Anbindung überprüft; dazu sind keine Scheinimplementierungen oder neuen Produkttests erforderlich.

## E2E-Abdeckung

| Benutzerfluss / Akzeptanzkriterium | Geplanter E2E-Test | Status |
|------------------------------------|--------------------|--------|
| AK1–6: Auskunftsgrundlage ohne fachliche UI | Keine nativen UI-E2E; echte HTTP-Proben sowie deterministische Adapter-/Service-/Modelltests | Nicht erforderlich mit Begründung: Der ursprüngliche Projektschritt schließt UI-Änderungen ausdrücklich aus. Native UI-Flüsse beginnen in späteren Schritten. |

## Fehlende oder unvollständige Planbestandteile

- [ ] AK3: Im Ablauf „Regionale Priorität und NRW-Echtzeitkonsolidierung“, Punkte 4/5, die Trennung von Haltestellen- und Fahrtzuordnung klarstellen. DHID identifiziert eine Haltestelle; die bislang ausdrücklich nur im Ersatzpfad genannten Fahrtmerkmale müssen auch bei gemeinsamer DHID eine eindeutige passende Fahrt belegen. Der Plan darf nicht als alleiniger DHID-Merge implementiert werden.
- [ ] AK6: Eine ausführbare Aufgabe für dauerhafte Betriebs-/Erweiterungsdokumentation mit konkretem Ausgabepfad aufnehmen, einschließlich Austausch/Ergänzung von Verbünden, Grenzen späterer Sharing-/Push-Anbindung, Konfiguration und Entwicklungs-/Produktivgrenzen. In der Umsetzungsreihenfolge und Tasks fehlen bislang die Erweiterungsdokumentation und ihre Abnahme.

## Hinweise

Geprüft wurden die vollständige Feature-Anforderung, inventory.md mit logic.md/models.md/tests.md, plan.md, die Tasks-Datei und die unveränderte ursprüngliche Schritt-1-Anforderung im Projektplan. Prüfbasis ist der Projekt-Basisbranch `task/issue-1-5142e36de8e0466382eb99c1599a0b31-ios-oepnv-app`; aktiver Schritt-Branch ist dessen Suffix `-schritt-1-fahrplanauskunft`. Dies ist eine Planprüfung, kein Implementierungs- oder API-Erfolgsnachweis.

Providerfeldbeschaffung, Live-Erreichbarkeit und Beschaffung eines echten NRW-Polygons sind zurecht als technische Implementierungsarbeit eingeplant. Es bedarf dafür keiner neuen Produktfrage oder erschöpfenden API-Forschung vor der Umsetzung. Der transparente bundesweite EFA-Entwicklungsfallback bei db-rest-503 ist im Rahmen des Auftrags zulässig; Produktionsfreigabe und flächendeckende Verfügbarkeit werden nicht zugesagt.

Die tatsächlichen Codebereiche FlowNRW.Core, FlowNRW.Tests und FlowNRW/MauiProgram sind berücksichtigt. Windows-Test-/Releasekonfiguration bleibt erhalten; iOS-CI und neue Deployment-Automation werden nicht eingeführt. Core-Zeilenabdeckung ≥70 % ist explizit eingeplant.

Kleine redaktionelle Inkonsistenz: Tasks-Zeile 12 nennt IVrrEfaProvider, der Plan dagegen IEfaProvider. Beim Nachplanen einheitlich benennen; daraus allein folgt keine fachliche Lücke.
