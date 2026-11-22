# Anforderung – Schritt 3: Vordergrund-Aktualisierung und Anlass-Zuordnung zentralisieren

Quelle: Issues #22 (Befund B2) und #24 (Befund B4) aus dem außerordentlichen Review #19;
Projektschritt 3 gemäß `docs/projects/task/issue-24-…-refresh-anlass-schritte-an-fav/project-plan.md`.

## Anforderungstext (übersetzt)

Beim Zurückkehren der App in den Vordergrund sollen veraltete Daten erneuert werden,
sofern die automatische Aktualisierung geladen und ein Intervall aktiv ist. Diese Regel
und die zugehörige Sequenz werden heute von vier Anzeige-Seiten (Start/Favoriten,
Abfahrtsmonitor, Verbindungsdetails, Verbindungsergebnisse) jeweils selbst
zusammengesetzt — mit divergierendem Schutz gegen überlappende Ausführung und
unterschiedlichem Abbruchverhalten: Nur zwei der vier Seiten verhindern überlappende
Aktualisierungen und führen den Abgleich der Aktualisierungsschleifen durch.

Zusätzlich liegt die Zuordnung der Aktualisierungs-Anlässe zu den fachlichen
Aktualisierungsschritten — Anwendungsstart, neu hinzugekommene Favoriten-Karten,
Rückkehr in den Vordergrund — allein als Wissen bei den aufrufenden Seiten
(`FavoriteHomeViewModel` exportiert `RefreshMissingAsync`, `RefreshAtStartupAsync`,
`RefreshStaleAsync`; `HomePage` mappt OnAppearing→Startup, Cards-Changed→Missing,
Foreground→Stale).

Es soll einen einzigen zentralen Einstieg im Aktualisierungs-Lebenszyklus geben, der die
Vordergrund-Rückkehr behandelt: Er kapselt die Regel „automatische Aktualisierung
geladen und Intervall aktiv", den Schutz gegen überlappende Ausführung einschließlich
Abbruchmöglichkeit und die Zuordnung der Aktualisierungs-Anlässe zu den jeweiligen
Aktualisierungsschritten. Die vier Seiten melden nur noch „sichtbar" bzw. „verdeckt"
und komponieren keine Aktualisierungssequenz mehr selbst; beim Verdecken müssen laufende
Aktualisierungen weiterhin zuverlässig abgebrochen werden. Da die zentrale Stelle heute
nur die Favoriten-Karten kennt, ist die Anbindung der übrigen aktualisierbaren Bereiche
(einzelner Haltestellen-Monitor, Verbindungssuche/-details/-ergebnisse) Teil dieses
Schritts, sodass alle bisherigen Resume-Pfade denselben geschützten Einstieg nutzen.

Die fachlichen Aktualisierungsschritte selbst, die Hintergrund-Aktualisierung und die
seiten-seitigen Aktualisierungsschleifen bleiben in ihrem Verhalten unverändert —
verändert wird nur, wer die Sequenz zusammensetzt und die Nebenbedingungen absichert.
Bestehende Tests zum Lebenszyklus sind zu erweitern bzw. anzupassen, sodass die
zentrale Regel, der Überlappungsschutz und die Anlass-Zuordnung für alle angebundenen
Bereiche nachgewiesen sind.

## Verbindliche Einzelregeln

1. Ein einziger geschützter Vordergrund-Einstieg (z. B. `ResumeForegroundAsync` am
   `RefreshLifecycle` oder eine dedizierte zentrale Stelle), der das
   `IsLoaded && IntervalSeconds != 0`-Gate, den Overlap-Guard und die
   Anlass→Schritt-Zuordnung kapselt.
2. Pages melden nur noch sichtbar/verdeckt; kein selbst komponiertes
   Refresh-Gate/-Sequenz-Mehr in Page-Code.
3. Beim Verdecken/Verlieren des Vordergrunds laufende Resume-Aktualisierungen
   zuverlässig abbrechen (bisheriges Verhalten mindestens gleichwertig).
4. Alle vier bisherigen Resume-Pfade anbinden: Home/Favoriten (`RefreshStaleAsync`),
   Stop-Monitor (`RefreshIfStaleAsync`), Verbindungsdetails + -ergebnisse
   (`Session.RefreshIfStaleAsync`). Die zwei bisher ungeschützten Pfade
   (JourneyDetail, Results) erhalten denselben Overlap-Schutz — das ist die
   beabsichtigte Verhaltensänderung.
5. Anlass-Zuordnung für Favoriten zentralisieren: Start → vollständige Anfangs-
   aktualisierung, neue Karten → fehlende Karten, Vordergrund → veraltete Karten.
   Das Methoden-Triplik am `FavoriteHomeViewModel` entfällt oder geht hinter einer
   anlassbezogenen Fassade auf.
6. Verhaltensneutralität für: fachliche Refresh-Schritte selbst, `RunBackgroundAsync`,
   `RefreshLoop`-Steuerung (Intervalle/Seiten-Schleifen bleiben seitenverantwortlich).
7. Tests: zentrales Gate, Overlap-Schutz, Abbruch beim Verdecken und
   Anlass→Schritt-Zuordnung für alle angebundenen Bereiche nachweisen.

## Priorität / Einordnung

Letzter Projektschritt; schließt Issues #22 und #24 im finalen PR gegen `staging`.
