# Inventur: partielle Providerergebnisse

Diese Inventur begrenzt sich auf die für die neue Anforderung relevanten Mergepfade. Die fachliche Eingabe steht in [requirement.md](requirement.md); die Abweichung ist in [acceptance-schritt-1.md](../../../projects/task/issue-1-5142e36de8e0466382eb99c1599a0b31-ios-oepnv-app/acceptance-schritt-1.md) dokumentiert.

## Ergebnis

- `ProviderOrchestrator.Execute` ruft bei regionaler Priorität beide bereits konfigurierten Provider auf und übergibt beide Mengen an `MergeJourneys` beziehungsweise `MergeEvents`.
- `MergeJourneys` bearbeitet derzeit ausschließlich regionale Primärfahrten. Nationale zusätzliche Fahrten werden nicht angehängt.
- `MergeEvents` und `RealtimeConsolidator` erhalten regionale Echtzeitwerte konservativ, geben aber nur die geplante Primärmenge zurück. Zusätzliche nationale Abfahrten fehlen dadurch ebenfalls.
- Identitätsfelder für Stop, Fahrt, Linie, Betreiber, Richtung und Sollzeit sind vorhanden. `TransitProviderOptions.MaxResults` ist vorhanden, wird in diesen privaten Mergefunktionen aber noch nicht als Vereinigungsgrenze angewandt.
- Die bestehende Testbasis deckt Priorität, Fallback, Abbruch, Cache und Identitätsmehrdeutigkeiten ab; der konkrete partielle A+B-Fall für Routing und Abfahrten fehlt.

Die gezielten technischen Details und Nachweise stehen in [inventory/logic.md](inventory/logic.md), [inventory/models.md](inventory/models.md) und [inventory/tests.md](inventory/tests.md).

