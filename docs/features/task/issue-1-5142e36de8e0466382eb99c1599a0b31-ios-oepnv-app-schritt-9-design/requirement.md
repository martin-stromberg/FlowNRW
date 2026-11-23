# Anforderung – Visuelle Integration

### Schritt 9: Visuelle Integration und Abnahme des Designentwurfs

**Kundenanforderung:** Die fertige App soll modern wie der in design-draft.zip gelieferte Entwurf aussehen. Integriere die tatsächlich vorhandenen HTML-/Markdown-Referenzen in eine konsistente native MAUI-Oberfläche für die bereits umgesetzten Funktionen. Die konkrete visuelle Abnahme richtet sich nach docs/design/acceptance.md; reine Funktionsnachweise oder eine blaue Akzentfarbe genügen nicht.

**Betroffene Bereiche:** Gemeinsame Designressourcen, Navigation, Startseiten-/Einzelmonitore, Verbindungssuche/-details, Haltestellen/Karte, Einstellungen, Barrierearmut und visuelle Regression.

**Abhängigkeiten:** 2, 3, 4, 5, 6, 7, 8.

**Verbindliche Rahmenbedingungen:** Native C#/.NET MAUI, MVVM und DI erhalten; tatsächliche Referenzen unter docs/design/reference. Drei persistente Kernbereiche Abfahrten, Verbindungen und Haltestellen; Details als Unterseite, Einstellungen sekundär. Keine Tickets, aktive Reisebegleitung, Tracking, Sharing, Push, Auslastungs-/Stationsausstattungsfiktionen oder andere Zusatzprodukte. Keine Providerneuimplementierung, keine Änderung der fachlichen Datenwahrheit. Windows-Release/-Tests in GitHub Actions bleiben; kein IIS, keine iOS-CI und kein automatisiertes Deployment. Native iOS-Build-/Geräteabnahme liegt beim Nutzer und wird durch konkrete visuelle/bedienbezogene Anleitung unterstützt. Bestehende fachliche Abnahmen bleiben gültig; betroffene UI-Flüsse werden nach Umbau regressionsgeprüft.

**Akzeptanzkriterien:**

1. Zentrale Farben, Typografie, Abstände, Radien, Linienbadges und Statusdarstellungen entsprechen den verbindlichen Entscheidungen in docs/design/acceptance.md und sind in hell/dunkel konsistent. Strukturiert gestaltete Abfahrts- und Verbindungskarten ersetzen die bisherige vorwiegend formular-/textbasierte Darstellung. Keine bloße Akzentfarbenänderung als Erfüllung.
2. Home/Monitor, Suche/Ergebnisse, Detailtimeline, Haltestellenliste/Karte und Einstellungen entsprechen der dort beschriebenen Bildhierarchie. Drei klar beschriftete Kernbereiche, Drilldowns und Zurück erhalten Eingaben/Auswahl; kein leerer oder funktionsloser Entwurfs-Tab. Bestehende manuelle/GPS-/Favoriten-/Intervall-/Kartenfunktionen bleiben erreichbar.
3. Kontrast, mindestens44×44 logische Touchziele, skalierbare Texte, Fokus, Safe Areas, Labels und nicht allein farbliche Echtzeit-/Fehlerzustände sind geprüft. Normale, leere, ladende und fehlerhafte Daten bleiben verständlich. Eine Karte ohne Position oder Echtzeit wird nicht dekorativ mit erfundenen Daten gefüllt.
4. Vollständige Screenshotmatrix aus docs/design/acceptance.md mit synthetischen Daten, identifizierbarem Commit, Größen/Themen/Skalierung und Referenzvergleich ist dauerhaft dokumentiert. Ein separater visueller Prüfer bewertet tatsächliche Appbilder gegen Referenzen; Abweichungen sind behoben oder mit konkretem technischem Versuchsnachweis offengelegt. Windows mindestens schmal430×900 und breit1024×768, hell/dunkel und große Schrift soweit möglich tatsächlich prüfen. Keine sensiblen Standortbilder, keine Teststeuerungen in finalen Abnahmebildern.
5. Native Windows-Regressionsflüsse für Navigation, Routing/Details, Haltestellensuche/GPS, Karte/Listenalternative→Monitor, Favoriten, manuelle/automatische Aktualisierung und Einstellungen bestehen nach Gestaltung. Format, Windows-Build mit Warnungen als Fehler, relevante Tests und mindestens70 % Corecoverage bleiben erfolgreich. Native iOS-Bild-/Geräteabnahme bleibt explizit offen beim Nutzer; Anleitung deckt Safe Areas, Themen, Dynamic Type und VoiceOver ab.
6. Tokens/Referenzentscheidungen, visuelle Nachweise und noch vorhandene Plattformgrenzen sind unter docs/help/design dokumentiert. Projektgesamtabschluss ist erst nach unabhängiger fachlicher und visueller Abnahme dieses Schritts zulässig.


Verbindliche Detailmatrix: docs/design/acceptance.md. Nutzerentscheidungen sind geklärt; keine offenen fachlichen Fragen. Agenten derzeit am Nutzungslimit; lokale Phasen gemäß Skillfallback. Die unabhängige visuelle Abschlussprüfung darf nicht als durchgeführt behauptet werden, solange kein separater Prüfer erfolgreich gearbeitet hat.
