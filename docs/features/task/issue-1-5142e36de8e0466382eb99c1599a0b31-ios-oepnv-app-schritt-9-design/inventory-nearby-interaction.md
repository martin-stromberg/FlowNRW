# Bestandsaufnahme: Auswahl einer nahen Haltestelle auf der Startseite

**Status:** Fehlerursache bestätigt, Produktcode unverändert.

## Reproduzierbarer Ablauf

1. `HomePage.OnAppearing` lädt über `FavoriteHomeViewModel.RefreshNearbyAsync` die Haltestellen zur aktuellen Position.
2. `FavoriteHomeViewModel` erstellt für jedes Ergebnis ein neues `Address`-Objekt und veröffentlicht es über `NearbyStops`.
3. `HomePage.RenderNearby` erzeugt daraus einen `NearbyStop{n}`-Button. Dessen Command ruft `monitor.OpenAsync(candidate)` auf.
4. `StopMonitorViewModel.OpenAsync` beendet sich sofort, wenn der Kandidat nicht als **dieselbe Objektinstanz** in `StopMonitorViewModel.Stops` enthalten ist:

   ```csharp
   if (opening || !Stops.Any(item => ReferenceEquals(item, candidate)) || candidate.Stop is null) return;
   ```

`StopMonitorViewModel.Stops` enthält jedoch nur die Treffer der Haltestellensuche bzw. dessen eigene Umgebungssuche. Die von `FavoriteHomeViewModel` erzeugten `Address`-Instanzen gehören zu keiner dieser Listen. Der Command wird damit korrekt ausgelöst, führt aber absichtlich ohne Navigation und ohne sichtbares Feedback zurück. Das erklärt die Rückmeldung exakt.

## Ursache und Abgrenzung

Die Mitgliedschaftsprüfung ist für Suchergebnisse sinnvoll: Sie verhindert, dass veraltete oder fremde `Address`-Objekte den Monitor öffnen. Sie ist für Startseiten-Ergebnisse aber zu eng, weil diese aus einem zweiten, unabhängigen Suchkontext stammen.

Die technische `Stop`-Identität ist in den Startseiten-Kandidaten vollständig vorhanden. `RefreshNearbyAsync` filtert bereits auf `item.Stop is not null`; die Fixture liefert beispielsweise `Id`, `Source`, Name und Koordinate. Navigation selbst ist funktionsfähig: `OpenStopAsync` setzt `SelectedStop`, navigiert mit `IDepartureNavigation.ShowMonitorAsync()` zur Route `departures` und lädt anschließend die Abfahrten.

## Minimaler Fix

Eine explizite Monitor-Methode für verifizierte externe Kandidaten ergänzen, etwa:

```csharp
public Task OpenNearbyFromHomeAsync(Address candidate) =>
    candidate.Stop is { Id: not "", Source: not "" } stop
        ? OpenStopAsync(stop)
        : Task.CompletedTask;
```

`HomePage.RenderNearby` ruft diese Methode statt `OpenAsync(candidate)` auf. Die bestehende öffentliche Methode `OpenAsync` und ihre Referenzprüfung bleiben für die Haltestellensuche unverändert. Die neue Methode akzeptiert ausschließlich eine vollständige technische `Stop`-Identität und nutzt danach denselben privaten, bereits bewährten Öffnungsweg.

Alternativ könnte `OpenAsync` einen zusätzlichen Herkunftsnachweis erhalten. Das würde aber zwei unabhängige Ergebnislisten in der vorhandenen Mitgliedschaftslogik verschränken und ist größer sowie fehleranfälliger als eine klar benannte Eingangsgrenze für Startseiten-Kandidaten.

## Fehlende Tests und Testplan

Der aktuelle Windows-Designlauf erfasst die Startseite und die nahen Haltestellen nur als Screenshots. Er klickt keinen `NearbyStop{n}`-Button. Seine Monitor-Checks betreffen Suchtreffer (`StopMatch0`) und Favoriten (`OpenFavorite{n}`); deshalb konnte der Regressionstest grün sein.

Nach dem Fix ergänzen:

1. **Core-Unit-Test:** Eine vollständige fremde `Address` mit `Stop(Id, Source)` über die neue Methode öffnen. Nach Aufruf müssen Navigation einmal ausgelöst, derselbe `Stop` an `IDepartureService` übergeben und die Abfahrt geladen werden. Ein Kandidat ohne Id oder Source darf weder navigieren noch einen Provider-Aufruf auslösen.
2. **Windows-UI-Test:** Fixture-Standort aktivieren, die Startseite öffnen, auf `NearbyStop0` klicken und auf `MonitorStop` sowie `MonitorStatus = manuell aktualisiert` warten. Zusätzlich `MonitorMetadata` auf `fixture-nearby-0` prüfen. Damit wird der vollständige reale UI-Pfad abgesichert.
3. **Regression:** Die bestehenden Tests für `StopMatch0` und `OpenFavorite{n}` ausführen; ihre Mitgliedschafts- bzw. Stale-Snapshot-Schutzmechanismen dürfen unverändert bleiben.
4. **Manuelle iOS-Abnahme:** Nach dem Geräte-Deploy eine nahe Haltestelle auf der Startseite tippen. Erwartung: Der Abfahrtsmonitor öffnet sich sofort mit Name, Ladezustand und anschließend den aktuellen Abfahrten bzw. einem eindeutigen Fehlerzustand.
