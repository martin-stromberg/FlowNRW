# Logik

## `ClickCounter`

Datei: `FlowNRW.Core/ClickCounter.cs`

Die Klasse ist Vorlagenlogik und implementiert keine ÖPNV-Datenversorgung.

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---------|-------------|------------------|
| `Click()` | `public` | Erhöht `Count` und gibt die formatierte Beschriftung zurück. |
| `FormatCaption(int count)` | `public static` | Formatiert die Singular-/Plural-Beschriftung und weist negative Werte zurück. |

Abonnierte Events: keine.

Publizierte Events: keine.

Weitere für die Anforderung relevante Routing-, Haltestellen-, Abfahrts-, Echtzeit-, Provider-, Cache- oder Diagnoseklassen wurden nicht gefunden. `FlowNRW/MauiProgram.cs` baut die MAUI-App, registriert aber keine fachlichen Services. `FlowNRW/App.xaml.cs` erzeugt ein `AppShell`; `FlowNRW/AppShell.xaml` verweist auf die Vorlagen-`MainPage`.
