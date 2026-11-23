namespace FlowNRW.Core.Transit;

/// <summary>Contract for TransitDiagnostics.</summary>
public interface ITransitDiagnostics
{
    /// <summary>Record technical metadata only.</summary>
    /// <param name="provider">provider input.</param>
    /// <param name="duration">duration input.</param>
    /// <param name="status">status input.</param>
    /// <param name="count">count input.</param>
    void Record(string provider, TimeSpan duration, string status, int count);

}
