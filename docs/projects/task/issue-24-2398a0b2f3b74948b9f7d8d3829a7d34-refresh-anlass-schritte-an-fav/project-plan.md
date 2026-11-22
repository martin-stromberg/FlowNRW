# Projektplan: Gemeinsame Refresh-/Restore-Fassaden (Befunde B1–B4 aus Review #19)

## Übersicht

Die vier Befunde aus dem außerordentlichen Code-Review #19 betreffen inoffizielle
Ausführungspfade und duplizierte Kompositionsregeln im Bereich Favoriten-Wiederherstellung
und automatischer Aktualisierung. Ziel ist, dass jeder der betroffenen Geschäftsvorgänge
genau einen zentralen Einstiegspunkt erhält, an dem die fachlichen Nebenbedingungen
abgesichert sind, und die divergierenden Aufrufer-Kompositionen entfallen. Betroffen sind
die Persistenz des Abfahrts-Caches, die Wiederherstellung persistierter Favoriten-Boards
(Start-Übersicht und Haltestellen-Monitor) sowie der Vordergrund-/Anlass-Lebenszyklus der
automatischen Aktualisierung über alle vier Anzeige-Seiten hinweg.

## Grobe Vorgehensentscheidungen

| Bereich | Entscheidung | Begründung |
|---------|-------------|------------|
| Reihenfolge | Der kleine, unabhängige Aufräumschritt an der Cache-Ablage wird zuerst umgesetzt, danach die gemeinsame Wiederherstellungs-Fassade, zuletzt die Zentralisierung der Vordergrund-Aktualisierung. | Der erste Schritt ist risikoarm und entkoppelt; die Restore-Fassade baut anschließend auf einer verschlankten Schnittstelle auf; die Vordergrund-Zentralisierung profitiert fachlich von der vorher geschaffenen gemeinsamen Fassade. |
| Fassaden-Ort Wiederherstellung | Als Orientierung empfiehlt die Bestandsaufnahme einen dedizierten, plattformunabhängigen Lese-Dienst in der Kernlogik als einzigen Einstiegspunkt, den beide bisherigen Aufrufpfade gemeinsam nutzen. | Beide Konsumenten bräuchten nur noch diese eine Abhängigkeit; die Prüfung ist ohne die Start-Übersicht testbar und entspricht der Konvention, Logik in der Kernlogik zu halten. Die konkrete technische Wahl trifft der Entwicklungsschritt selbst. |
| Stale-Semantik | Zielsemantik ist „veraltete Boards anzeigen und als veraltet markieren" — veraltete Boards werden nicht verworfen. | Das ist die fachlich bessere Variante, entspricht dem Verhalten der Start-Übersicht und ist in der Anforderung so festgelegt. |
| Behandlung zeitloser Fahrten | Zeitlose Fahrten (ohne wirksame Abfahrtszeit) werden beim Wiederherstellen einheitlich verworfen. | Diese Filterung gilt bereits in beiden bisherigen Wiederherstellungspfaden; die abweichende Behandlung existiert nur im Live-Aktualisierungspfad, der nicht Gegenstand der Harmonisierung ist. |
| Anlass-Zuordnung | Die Zuordnung „Aktualisierungs-Anlass → Aktualisierungs-Schritt" wird zusammen mit der Vordergrund-Regel an der zentralen Stelle im Aktualisierungs-Lebenszyklus gebündelt; die Befunde B2 und B4 werden in einem Schritt mitaufgeräumt. | Beide Befunde betreffen dieselbe Aufrufer-Komposition an denselben Stellen; ein gemeinsamer Schritt vermeidet doppelte Umbauten an den vier Seiten. |
| Umfang | Es wird nur harmonisiert und zentralisiert — kein neues Refresh-Verhalten, kein UI-Umbau, die fachlichen Aktualisierungsschritte selbst bleiben erhalten. | Explizites Nicht-Ziel der Anforderung; begrenzt das Risiko der Umstellung. |

## Entwicklungsschritte

### Schritt 1: Ungenutzten Schreibvorgang aus der Cache-Ablage entfernen

**Beschreibung:** Die Ablage für zwischengespeicherte Abfahrts-Boards bietet aktuell einen
Schreibvorgang an, der den kompletten Cache-Bestand in einem Schritt ersetzt. Dieser
Vorgang wird produktiv nie aufgerufen — produktiv werden Cache-Einträge ausschließlich
einzeln eingefügt bzw. aktualisiert sowie gezielt oder als verwaiste Reste entfernt. Der
ungenutzte Voll-Ersetzungs-Schreibvorgang soll aus der öffentlichen Schnittstelle der
Cache-Ablage entfernt oder so gekapselt werden, dass er von außen nicht mehr erreichbar
ist, sodass die Schnittstelle nur noch die tatsächlich produktiv genutzten Zugriffswege
anbietet. Bestehende Tests, die den Voll-Ersetzungs-Pfad direkt prüfen oder Prüfdoubles
mit diesem Vorgang bereitstellen, sind anzupassen: Die bisher über den ungenutzten Pfad
charakterisierten Eigenschaften (Rundreise, Integrität, Validierung) sind, soweit fachlich
relevant, über den produktiven Einzel-Einfüge-/Aktualisierungspfad abzudecken. Das
Ändern einzelner Einträge, das Entfernen einzelner Einträge und die Bereinigung
verwaister Einträge bleiben unverändert verfügbar und getestet.

**Abhängigkeiten:** Keine

**Betroffene Bereiche:** Persistenz des Abfahrts-Caches (Cache-Ablage-Schnittstelle),
Testabdeckung der Persistenz.

### Schritt 2: Einheitlicher Einstieg zum Wiederherstellen persistierter Favoriten-Boards

**Beschreibung:** Ein persistiertes Favoriten-Board wird heute auf zwei divergierenden
Wegen wiederhergestellt: In der Start-Übersicht werden beim Laden alle gespeicherten
Favoriten-Karten aus dem Cache gefüllt, wobei ein veraltetes Board angezeigt und als
veraltet markiert wird; beim direkten Öffnen eines einzelnen Haltestellen-Monitors wird
dagegen bei jedem Öffnen sowohl die Favoriten- als auch die Cache-Datei vollständig neu
gelesen (ohne Abbruchmöglichkeit und ohne gemeinsame Sperre mit den Schreibvorgängen),
und ein veraltetes Board wird verworfen statt angezeigt. Es soll genau einen
Einstiegspunkt für den Geschäftsvorgang „persistiertes Board einer gespeicherten
Favoriten-Haltestelle wiederherstellen" geben, der sämtliche Nebenbedingungen zentral
bündelt: Prüfung der Favoritenzugehörigkeit, gebündeltes Lesen des Cache-Bestands statt
wiederholten Lesens pro Öffnen, Bewertung der Datenfrische, einheitliche Filterung der
Fahrten (zeitlose Fahrten werden verworfen, nur künftige Fahrten werden gezeigt) und
einheitliche Stale-Semantik — veraltete Boards werden angezeigt und als veraltet
markiert, nicht verworfen. Beide bisherigen Aufrufpfade (Start-Übersicht und
Haltestellen-Monitor) nutzen diesen einen Einstieg; die Eigenkompositionen entfallen,
einschließlich der mehrfach duplizierten Ermittlung der wirksamen Abfahrtszeit. Ein Board,
das keine gültigen künftigen Fahrten mehr enthält, liefert weiterhin sein
Linieninventar, damit die Karte ihre Linienanzeige behält; die Bereinigung verwaister
Cache-Einträge bleibt Verantwortung des initialen Ladevorgangs der Favoriten-Übersicht.
Bestehende Tests, die die alte divergente Monitor-Semantik pinnen (veraltetes Board wird
nicht angezeigt), sind auf die einheitliche Semantik „veraltete Boards anzeigen und
markieren" anzupassen; die Testabdeckung ist so zu erweitern, dass beide Aufrufpfade
nachweislich denselben Einstieg und dieselbe Semantik verwenden.

**Abhängigkeiten:** 1

**Betroffene Bereiche:** Favoriten-Wiederherstellung (Start-Übersicht und
Haltestellen-Monitor), Abfahrts-Cache, Testabdeckung der Wiederherstellung.

### Schritt 3: Vordergrund-Aktualisierung und Anlass-Zuordnung zentralisieren

**Beschreibung:** Beim Zurückkehren der App in den Vordergrund sollen veraltete Daten
erneuert werden, sofern die automatische Aktualisierung geladen und ein Intervall aktiv
ist. Diese Regel und die zugehörige Sequenz werden heute von vier Anzeige-Seiten
(Start/Favoriten, Abfahrtsmonitor, Verbindungsdetails, Verbindungsergebnisse) jeweils
selbst zusammengesetzt — mit divergierendem Schutz gegen überlappende Ausführung und
unterschiedlichem Abbruchverhalten: Nur zwei der vier Seiten verhindern überlappende
Aktualisierungen und führen den Abgleich der Aktualisierungsschleifen durch. Zusätzlich
liegt die Zuordnung der Aktualisierungs-Anlässe zu den fachlichen Aktualisierungsschritten
— Anwendungsstart, neu hinzugekommene Favoriten-Karten, Rückkehr in den Vordergrund —
allein als Wissen bei den aufrufenden Seiten. Es soll einen einzigen zentralen Einstieg
im Aktualisierungs-Lebenszyklus geben, der die Vordergrund-Rückkehr behandelt: Er kapselt
die Regel „automatische Aktualisierung geladen und Intervall aktiv", den Schutz gegen
überlappende Ausführung einschließlich Abbruchmöglichkeit und die Zuordnung der
Aktualisierungs-Anlässe zu den jeweiligen Aktualisierungsschritten. Die vier Seiten
melden nur noch „sichtbar" bzw. „verdeckt" und komponieren keine Aktualisierungssequenz
mehr selbst; beim Verdecken müssen laufende Aktualisierungen weiterhin zuverlässig
abgebrochen werden. Da die zentrale Stelle heute nur die Favoriten-Karten kennt, ist die
Anbindung der übrigen aktualisierbaren Bereiche (einzelner Haltestellen-Monitor,
Verbindungssuche und -details) Teil dieses Schritts, sodass alle bisherigen
Resume-Pfade denselben geschützten Einstieg nutzen. Die fachlichen
Aktualisierungsschritte selbst, die Hintergrund-Aktualisierung und die Seiten-seitigen
Aktualisierungsschleifen bleiben in ihrem Verhalten unverändert — verändert wird nur,
wer die Sequenz zusammensetzt und die Nebenbedingungen absichert. Bestehende Tests zum
Lebenszyklus sind zu erweitern bzw. anzupassen, sodass die zentrale Regel, der
Überlappungsschutz und die Anlass-Zuordnung für alle angebundenen Bereiche nachgewiesen
sind.

**Abhängigkeiten:** 2

**Betroffene Bereiche:** Automatische Aktualisierung / Vordergrund-Lebenszyklus, alle vier
Anzeige-Seiten (Start/Favoriten, Abfahrtsmonitor, Verbindungssuche und -ergebnisse),
Testabdeckung des Aktualisierungs-Lebenszyklus.

## Übersichtstabelle

| # | Titel | Abhängigkeiten | Betroffene Bereiche |
|---|-------|-----------------|----------------------|
| 1 | Ungenutzten Schreibvorgang aus der Cache-Ablage entfernen | Keine | Persistenz des Abfahrts-Caches, Testabdeckung |
| 2 | Einheitlicher Einstieg zum Wiederherstellen persistierter Favoriten-Boards | 1 | Favoriten-Wiederherstellung, Abfahrts-Cache, Testabdeckung |
| 3 | Vordergrund-Aktualisierung und Anlass-Zuordnung zentralisieren | 2 | Aktualisierungs-Lebenszyklus, alle vier Anzeige-Seiten, Testabdeckung |

## Offene Punkte

Keine.
