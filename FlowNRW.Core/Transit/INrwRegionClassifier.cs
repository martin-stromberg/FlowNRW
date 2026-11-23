namespace FlowNRW.Core.Transit;

/// <summary>Contract for NrwRegionClassifier.</summary>
public interface INrwRegionClassifier
{
    /// <summary>Classify against the embedded official NRW polygon.</summary>
    /// <param name="coordinate">coordinate input.</param>
    /// <returns>Operation result.</returns>
    bool IsInNrw(GeoCoordinate? coordinate);

}
