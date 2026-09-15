# Amtliche NRW-Grenze

- Herkunft: GeoBasis NRW, DVG2 Landesgrenze NRW, amtlicher ArcGIS Hosted/Verwaltungsgrenzen FeatureServer Layer 0.
- Datenstand im Attribut stand: 2023-06-12; KN 05000000, Name Nordrhein-Westfalen.
- Abruf: 2026-09-08, GeoJSON-Ausgabe in WGS84 (EPSG:4326), unvereinfachtes MultiPolygon des Dienstes, einschließlich Ringen und Teilpolygonen.
- Ressource: FlowNRW.Core/Transit/Resources/nrw-dvg2-2023.geojson
- SHA-256: 431335C7B612B9E70F0CF6F6F5244AC6641C404717620566F441D9C53C2BDBE7
- Lizenz: Datenlizenz Deutschland – Zero – Version 2.0. Die offizielle Produktseite nennt Nutzung ohne Einschränkungen/Bedingungen.
- Produkt/Lizenznachweis: https://www.opengeodata.nrw.de/produkte/geobasis/vkg/dvg/dvg2/
- Geometrieabruf: https://www.arcgishostedserver.nrw.de/arcgis/rest/services/Hosted/Verwaltungsgrenzen/FeatureServer/0/query?where=1%3D1&outFields=*&outSR=4326&f=geojson
- Metadaten: https://www.arcgishostedserver.nrw.de/arcgis/rest/services/Hosted/Verwaltungsgrenzen/FeatureServer/0?f=pjson

Der Klassifikator nutzt Point-in-Polygon, berücksichtigt Löcher, klassifiziert Außenring-Grenzpunkte als NRW und erkennt unbekannte Koordinaten als nicht regional. Tests umfassen Essen, Aachen, Bielefeld und Münster sowie Osnabrück, Koblenz, Maastricht und Berlin. VRR-Gebiet und NRW-Boundingbox sind keine Ersatzgrenze. DVG2 ist eine generalisierte Verwaltungsgrenze, keine zentimetergenaue Katastergrenze. Die gespeicherte Version ist explizit und aktualisierbar; neue Verwaltungsgrenzen erfordern Ressourcen- und Testaktualisierung.
