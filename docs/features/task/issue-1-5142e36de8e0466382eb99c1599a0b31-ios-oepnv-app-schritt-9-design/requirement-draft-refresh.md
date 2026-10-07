# Anforderung – Aktualisierter UI-Entwurf und Nearby-Interaktion

**Erfasst:** 03.10.2026
**Status:** Verbindliche Ergänzung zu `requirement.md` und `correction-tasks.md`

## Anlass und Referenz

Die bisherigen Screenshots des ursprünglichen Entwurfs sind nicht als visuelle Referenz verwendbar, da sie nur leere 28-Byte-Dateien enthalten. Sie werden für die weitere Umsetzung und visuelle Abnahme ersetzt durch das im Repository abgelegte Archiv `stitch_nrw_transit_ios_app.zip`.

Die darin enthaltenen hellen und dunklen Ansichten sowie die beiden Design-System-Beschreibungen sind die neue visuelle Referenz. Besonders maßgeblich sind die Ansichten für Abfahrtsmonitor, Verbindungssuche, Fahrt-/Verbindungsdetail und Umgebungskarte/Haltestellen, jeweils in Hell- und Dunkelmodus.

## R1 – Interaktion mit nahegelegenen Haltestellen

Die auf der Startseite angezeigten nahegelegenen Haltestellen sind bedienbar.

- Tippen oder Klicken auf eine Haltestelle öffnet deren vorhandenen Abfahrtsmonitor.
- Der gewählte Haltestellenname und die vollständige interne Haltestellenidentität werden dabei übergeben.
- Der Zielmonitor zeigt einen verständlichen Lade-, Leer- oder Fehlerzustand, falls dafür keine Abfahrten vorliegen.
- Die Interaktion muss per Maus, Touch und Tastatur erreichbar sein; das Ziel hat mindestens 44×44 logische Pixel und einen zugänglichen Namen.

**Abnahme:** Ein synthetischer Nearby-Eintrag, der kein Favorit ist, öffnet nach Aktivierung den Monitor genau dieser Station. Der Windows-UI-Test deckt Maus/Tastatur oder Touch-Simulation dafür ab.

## R2 – Umsetzung anhand des aktualisierten Stitch-Entwurfs

Die vorhandene native MAUI-Oberfläche wird gegen den aktualisierten Entwurf weiterentwickelt. Das Ziel ist eine ruhige, moderne und freundliche ÖPNV-App statt einer Ansammlung technisch wirkender Formularfelder, Statuszeilen und gleichwertiger Buttons.

Für den bereits vorhandenen Funktionsumfang gelten insbesondere diese visuellen Leitlinien aus dem Archiv:

- Klar gegliederte Oberflächen mit gruppierten, großzügig gerundeten Karten, konsistentem Raster, ausreichend Weißraum und einer eindeutigen primären Aktion je Bereich.
- Inhaltliche Hierarchie: Haltestellen-/Zielname, Abfahrts- oder Verbindungszeit und Linieninformation stehen im Vordergrund; Quelle, Kennungen, technische Statuswerte und sekundäre Aktionen treten optisch zurück.
- Verkehrsarten werden über kompakte, kontrastreiche Linienbadges dargestellt. Echtzeit, Verspätung, Ausfall und fehlende Daten bleiben sachlich, gut lesbar und nicht ausschließlich durch Farbe erkennbar.
- Suche, Abfahrt/Ankunft und weitere vorhandene Auswahloptionen verwenden kompakte native Eingabe- und Auswahlmuster. Die UI zeigt ausschließlich Optionen, die im bestehenden Funktionsumfang tatsächlich funktionieren.
- Abfahrten, Verbindungssuche/-details und Haltestellen/Karte folgen jeweils der im Archiv gezeigten Informationshierarchie, ohne fachliche Daten zu erfinden.

## R3 – Heller und dunkler Modus

Die im Archiv gelieferten dunklen Referenzansichten gehören zur Abnahme.

- Alle überarbeiteten Bereiche sind in Hell- und Dunkelmodus konsistent gestaltet.
- Oberflächen, Karten, Textstufen, Separatoren, Eingaben, Linienbadges und Statusfarben behalten ausreichenden Kontrast.
- Der Dunkelmodus nutzt dunkle gruppierte Oberflächen und keine bloße Invertierung der hellen Ansicht.
- Der aktuelle Systemmodus und der bestehende Testmodus dürfen keine entgegengesetzte Darstellung erzwingen.

**Abnahme:** Die definierte Screenshotmatrix enthält die überarbeiteten Kernabläufe mindestens einmal in Hell und Dunkel; kein Text, Icon, Badge oder Eingabewert verschwindet in einem Modus.

## R4 – Elemente außerhalb des Anforderungskatalogs

Das Stitch-Archiv enthält auch Entwurfselemente ohne fachliche Grundlage im ursprünglichen Anforderungskatalog. Diese werden nicht als funktionslose Platzhalter umgesetzt. Sie werden aus den betroffenen Ansichten ausgelassen, sofern sie nicht bereits durch eine vorhandene Funktion gedeckt sind.

Das betrifft insbesondere Tickets, aktive Fahrtbegleitung bzw. Tracking, Kapazitätsanzeigen, Mikrofon-/QR-Funktionen, Push- und Störungsprodukte, Sharing-Angebote sowie weitere nicht vorhandene Tabs oder Aktionen. Bereits umgesetzte Navigation und fachliche Funktionen bleiben erhalten; die visuellen Muster des Entwurfs werden darauf übertragen.

## R5 – Qualitätssicherung und Dokumentation

- Die Entwurfsbilder werden aus `stitch_nrw_transit_ios_app.zip` in eine dauerhaft lesbare Referenzablage überführt; leere Altdateien sind nicht als Nachweis zu verwenden.
- Für R1 entstehen ein automatisierter Windows-UI-Nachweis und ein Screenshot des geöffneten Nearby-Monitors.
- Die bestehende Designmatrix wird für die aktualisierten Referenzen, helle/dunkle Ansichten und die betroffenen Home-, Monitor-, Suche-, Detail- und Kartenabläufe erneuert.
- Windows bleibt Test- und Releaseplattform. Die visuelle iOS-Geräteabnahme, einschließlich Safe Areas und Dynamic Type, bleibt beim Nutzer.

## Nicht-Ziele

- Keine Erweiterung des ursprünglichen fachlichen Umfangs und keine Simulation nicht vorhandener Daten.
- Keine Änderung von Providern, Echtzeitlogik, Routingfachlichkeit, CI oder automatisiertem Deployment allein wegen des Entwurfs.
- Keine Verwendung der alten leeren Screenshot-Dateien als Referenz oder Abnahmebeleg.
