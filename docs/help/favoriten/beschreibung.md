# Bedienung

Über **Haltestelle hinzufügen** eine Station suchen und deren Abfahrtsmonitor öffnen. **Als Favorit speichern** übernimmt die vollständige Haltestellenidentität. Nach der Rückkehr zur Startseite erscheinen die nächsten Abfahrten. Gespeicherte Favoriten bleiben nach einem App-Neustart erhalten.

Jede Tafel lässt sich separat über **Aktualisieren** laden. Ein langsamer Abruf blockiert die übrigen Tafeln nicht. Über den Chevron-Schalter rechts oben im Kartenkopf klappt eine Tafel ihre Abfahrten auf oder zu — ein erneutes Zuklappen ist ohne Scrollen erreichbar. Im zugeklappten Zustand zeigt die Tafel eine kompakte Zeile mit den Linienbadges und der nächsten Abfahrtszeit; der Linienbestand ergänzt sich aus jeder gelieferten Antwort, auch wenn sie nur teilweise Daten enthält.

Bei einem Fehler bleiben vorhandene Abfahrten mit ihrem Datenstand sichtbar. Nach einem erfolgreichen leeren Ergebnis werden frühere Abfahrten entfernt. Echte Fehler- und Leerzustände erscheinen weiterhin als Statuszeile. Liegt das angezeigte Ergebnis dagegen nur möglicherweise unvollständig oder veraltet vor (Anbieterwarnung, Ersatzquelle oder veralteter Cache), erscheint rechts neben der Statuszeile ein kleines Warnsymbol statt eines dauerhaften Hinweistexts. Erst ein Tipp auf das Symbol blendet Quelle, Datenstand und Provenienz ein; ein weiterer Tipp oder ein neuer einwandfreier Datenstand schließt die Details wieder.

**Entfernungen aktualisieren** fragt ausdrücklich die aktuelle Position ab und sortiert nach Luftlinie. Stationen ohne Koordinaten stehen am Ende. Bei gleicher Entfernung bleibt die Speicherreihenfolge erhalten. Ohne aktuelle Position zeigt die App unbekannte Entfernungen und die Speicherreihenfolge. Die aktuelle Nutzerposition wird nicht in der Favoritendatei gespeichert.

**Abfahrtsmonitor öffnen** zeigt die vollständige Tafel. **Favoriten auf Karte zeigen** bietet Karte und Stationsliste; auch Stationen ohne Kartenposition bleiben über die Liste erreichbar. **Verbindung suchen** öffnet die bisherige Start-/Zielsuche.

**Favorit entfernen** löscht eine Station. Im Einzelmonitor kann ein gespeicherter Favorit ebenfalls entfernt werden. Erst nach erfolgreicher Speicherung bestätigt die App die Änderung. Bei Speicherfehlern bleibt der bisherige Bestand erhalten; die Aktion kann erneut ausgeführt werden.

Die Startseite lädt fehlende Tafeln beim Öffnen. Über **Aktualisierung einstellen** lässt sich die [automatische Vordergrundaktualisierung](intervalle.md) konfigurieren.

Unter **Nächste Haltestellen** listet die Startseite weitere Haltestellen in der Umgebung der zuletzt angefragten Position — ohne interne Kennungen. Die Detailzeile nennt stattdessen die Entfernung: bevorzugt den vom Anbieter gelieferten Wert, sonst die als solche gekennzeichnete Luftlinie; ohne beides steht „Entfernung unbekannt". Ein Tipp auf einen Eintrag öffnet dessen Abfahrtsmonitor. Doppelt genannte Ortsnamen („Münster (Westf), Münster (Westf) Hbf") werden in allen Anzeigen als „Münster (Westf) Hbf" verkürzt; die zugrunde liegende Haltestellenidentität bleibt unverändert.
