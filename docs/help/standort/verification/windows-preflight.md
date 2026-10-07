# Windows-Standortvorprüfung – 17.09.2026

Vor Implementierung von Schritt 5 wurde ausschließlich lesend geprüft: Der Windows-Dienst `lfsvc` läuft. Keine Betriebssystemeinstellung wurde geändert, kein Standort angefordert und keine Koordinate gespeichert. Das belegt weder eine erteilte Standortberechtigung noch eine verfügbare Position.

Die App läuft unverpackt als Windows-Desktopanwendung. Die [offizielle MAUI-Geolocation-Dokumentation](https://learn.microsoft.com/en-us/dotnet/maui/platform-integration/device/geolocation?view=net-maui-10.0) beschreibt für Windows keine zusätzliche Einrichtung; iOS benötigt eine verständliche NSLocationWhenInUseUsageDescription. Der spätere tatsächliche Versuch muss in der regulären App erfolgen und vom deterministischen Fixture-Test getrennt protokolliert werden. Reale Koordinaten gehören nicht in dauerhafte Prüflogs.

Vorhandene `IStopSearchService.NearbyAsync` und NRW-/Cache-/Fallbacklogik werden wiederverwendet. `StopSearchService` führt auf derselben Instanz nur die jeweils neueste Operation fort; voneinander unabhängige Suchen brauchen getrennte Instanzen. Karten- und Monitorzuordnung dürfen die Originalstopidentität und die bestehende Mitgliedschaftsprüfung nicht umgehen.
