# Planprüfung – Favoriten, Haltestellensuche und Verbindungen

**Stand:** 04.10.2026
**Status:** Plan vollständig
**Prüfrunde:** 2, nach Überarbeitung der Befunde P1 bis P3.
**Geprüft:** `requirement-navigation-favorites.md`, `inventory-navigation-favorites.md`, `plan-navigation-favorites.md`; Datenschutzrahmen aus `issue.md` und `docs/projects/task/issue-1-5142e36de8e0466382eb99c1599a0b31-ios-oepnv-app/project-plan.md`.

## Anforderungsabdeckung

| Anforderung | Geplante Umsetzung | Geplanter Nachweis |
| --- | --- | --- |
| R1 – Farbig konsistente Linienchips, keine Zeitdopplung aufgeklappt, verständliche Symbole | Paket 1 trennt kompakte und aufgeklappte Ansicht, übernimmt den Linienchipstil und verlangt semantische Beschriftung und mindestens 44×44 große Touch-Ziele. | Projektionstests sowie native Umschalt-/Accessibility-Prüfung mit schmalem Screenshot. |
| R2 – Cacheanzeige und erhaltener Suchzustand | Pakete 2 und 3 trennen Trefferliste und Monitorwahl; Sessioncache und persistenter Favoritencache folgen derselben Anbieteridentität und Frischeprüfung. | Navigation zurück und erneutes Öffnen, verzögerter Abruf, Fehler/Abbruch, abgelaufene Daten, Quellenidentität, Eviction und spätes A-Ergebnis nach Wechsel zu B. |
| R3 – Verbindungsfavoriten | Pakete 4 und 5 legen einen separaten validierten Store, Ergebnisüberschrift mit Favoritenaktion sowie direkte Wiederverwendung beider Haltestellen fest. | Storetests und nativer Ablauf mit tatsächlichem Prozessneustart, Wiederwahl, Entfernung und erneutem Neustart. |
| R4 – Start/Ziel-Tausch | Paket 6 verlangt die vollständige atomare Endpunktoperation ohne Anfrage; Zeit-/Ankunftsparameter bleiben erhalten. | Sessiontests und native Fixturezähler vor/nach Favoritenauswahl und Swap; erst die bewusste Suche löst Routing aus. |
| R5 – Relevante Verbindungsdetails | Paket 7 prüft native Timeline, Karten und erreichbaren Fallback; Fahrtfolge, Umstieg, Gehweg, Ausfall und notwendige Hinweise bleiben erhalten. | Präsentationstests, native Screenshots und Accessibility-Texte für mehrteilige Fahrten sowie fehlende oder abweichende Daten. |

## Erledigte Befunde

- **P1 – Datenschutz und Suchcache:** Beliebige Suchhaltestellen bleiben jetzt im begrenzten Sessioncache. Nur ausdrücklich gespeicherte Stopfavoriten werden zusätzlich auf Datenträger geschrieben. Suchtext, Trefferliste und aktuelle Standortkoordinaten erhalten keine neue Persistenz. Die Grenzen sind auf 100 Haltestellen mit je höchstens 100 Abfahrten und 100 Linien festgelegt, mit LRU und Schutz des geöffneten Monitors. Sessioneviction und Favoriten-/Orphanbereinigung bleiben getrennt. Geplante Tests prüfen die JSON-Grenze und den Zustand nach Prozessneustart. Verbindungsfavoriten speichern nur ausdrücklich gewählte Haltestellenendpunkte.
- **P2 – Navigation und späte Antworten:** Auswahlrevision und Anbieteridentität werden vor jedem Cache-/Providerstart erfasst und vor UI-Übernahme erneut geprüft. Navigation zurück und Wechsel A → B invalidieren den alten Vorgang. Verspätete Cachetreffer dürfen neuere Providerdaten nicht ersetzen. Teilantworten, Fehler und Abbruch erhalten vollständige Bestände; erneute Cacheanzeige filtert inzwischen abgelaufene Abfahrten. Entsprechende Unit-/Integrations- und native Stopwechseltests sind verbindlich vorgesehen.
- **P3 – Native Wiederverwendung der Verbindungsfavoriten:** Paket 5 beschreibt nun Speichern, echten Prozessneustart mit derselben Testablage, Auswahl, Swap, bewusste Suche, Entfernen und einen zweiten Neustart. Fixturezähler belegen ausdrücklich, dass Auswahl und Swap keine Haltestellen-/Routinganfrage auslösen. Sichtbare Namen, interne Identitäten und unveränderte Zeitparameter sind Teil der Abnahme.
- **Redaktion:** Die Bestandsaufnahme bestätigt korrekt, dass nächste Uhrzeiten in eingeklappten Linienchips erwünscht sind und nur die zusätzliche Anzeige im aufgeklappten Zustand entfällt.

## Durchführung und Grenzen

Die Pakete sind ausreichend klein und ihre Abhängigkeiten nachvollziehbar. Paket 0 berücksichtigt die offenen Nachweise des vorangehenden Abfahrtpakets. Datenintegritätsbefunde müssen vor der Erweiterung geschlossen werden. Native Windows-Prozessläufe sind Bestandteil der Abnahme; bloß vorhandene Assertions oder nicht ausgeführte Szenarien gelten ausdrücklich nicht als Erfolg. iOS-Geräteabnahme bleibt entsprechend der Nutzeranweisung beim Nutzer.

Der Abschnitt „Offene technische Entscheidungen“ enthält bereits getroffene Cache-, Duplikat- und Migrationsentscheidungen sowie auszuführende Tests; daraus folgt keine offene fachliche Nutzerfrage. Die verbleibende konkrete Symbolwahl und Speichergrenze der Verbindungsfavoriten können während der Implementierung nach den vorgesehenen Begrenzungs-/Usabilityregeln getroffen und getestet werden.

## Ergebnis

Keine offenen Planlücken. Der Plan ist für die Implementierung freigegeben. Dies ist eine Dokumentenprüfung, keine Bestätigung bereits implementierter Funktionen oder ausgeführter Tests.
