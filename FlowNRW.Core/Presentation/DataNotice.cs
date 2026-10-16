using FlowNRW.Core.Transit;

namespace FlowNRW.Core.Presentation;

/// <summary>Shared derivation of the unobtrusive data-quality indicator for displayed provider results.</summary>
internal static class DataNotice
{
    /// <summary>Whether the result carries a data-quality notice worth an unobtrusive indicator.</summary>
    /// <typeparam name="T">Item type.</typeparam>
    /// <param name="result">Displayed provider response.</param>
    /// <returns>Whether warnings, a fallback source or stale data are present.</returns>
    internal static bool HasDataQualityNotice<T>(this ProviderResult<T>? result)
    {
        return result is { } value && (value.Warnings.Count > 0 || value.IsFallback || value.IsStale);
    }
}
