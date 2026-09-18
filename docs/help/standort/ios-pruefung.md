# iOS-Geräteprüfung: Standort

Diese Prüfung liegt wie vereinbart beim Nutzer. Ein erfolgreicher Windows-Test bestätigt weder einen iOS-Build noch eine Geräteprüfung. Die iOS-App verwendet ausschließlich `NSLocationWhenInUseUsageDescription`; eine Hintergrund- oder Always-Berechtigung gehört nicht zu diesem Schritt.

1. App mit Xcode/MAUI auf einem iPhone installieren. Ohne Standortaktion manuell Start und Ziel suchen; es darf kein Standortdialog erscheinen.
2. Bei Start **Aktuellen Standort verwenden** wählen, den deutschen Nutzungsgrund prüfen und zustimmen. Ziel manuell wählen, Verbindung suchen, Details und Rücknavigation prüfen. Danach denselben Ablauf mit Standort als Ziel durchführen.
3. Berechtigung in den iOS-Einstellungen entziehen und erneut anfordern. Verständlichen Hinweis und weiter nutzbare manuelle Suche prüfen. Wieder erteilen und Erholung bestätigen.
4. Genaue Position deaktivieren. Eine ungefähre Position darf genutzt werden, muss aber als solche gekennzeichnet sein. Die App darf keine Vollgenauigkeit erzwingen.
5. Ortungsdienste deaktivieren und eine Position anfordern. Danach Dienst wieder aktivieren und erneut versuchen. Auch bei fehlender Position oder Zeitüberschreitung muss die Bedienung frei werden.
6. **Haltestellen in meiner Nähe** öffnen. Passende Umgebung, Quellenangabe und Entfernungen prüfen. Je eine Auswahl aus Liste, Kartenmarker und Kartenliste muss den richtigen Monitor öffnen.
7. Während einer Standortabfrage manuell eingeben bzw. zurück navigieren. Späte Antworten dürfen keine neuere Auswahl ersetzen oder eine fremde Seite öffnen.
8. Große Schrift, VoiceOver, Hoch-/Querformat und Kartenbedienung prüfen. Buttons und Fehlertexte müssen lesbar und erreichbar bleiben.

Datum, iOS-Version, Gerät und Ergebnis je Ablauf festhalten. Keine privaten Koordinaten, Wohnortableitungen oder sensiblen Screenshots in öffentliche Prüfberichte übernehmen.
