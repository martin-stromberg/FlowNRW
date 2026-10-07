using System.Globalization;
using System.Text;
using System.Text.Json;

namespace FlowNRW.Core.Refresh;

/// <summary>Atomic storage of a JSON integer with a strict 64-byte read limit.</summary>
public sealed class JsonRefreshSettingsStore : IRefreshSettingsStore
{
    private readonly string path;
    private readonly SemaphoreSlim gate = new(1, 1);

    /// <summary>Creates storage at the platform-owned settings path.</summary>
    /// <param name="path">Settings file path.</param>
    public JsonRefreshSettingsStore(string path) { this.path = Path.GetFullPath(path); }

    /// <inheritdoc />
    public async Task<int?> LoadAsync(CancellationToken cancellationToken = default)
    {
        await gate.WaitAsync(cancellationToken);
        try
        {
            if (!File.Exists(path)) return null;
            await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 64, FileOptions.Asynchronous);
            if (stream.Length > 64) throw new InvalidDataException("Refresh settings exceed the size limit.");
            var bytes = new byte[65];
            var count = 0;
            while (count < bytes.Length)
            {
                var read = await stream.ReadAsync(bytes.AsMemory(count), cancellationToken);
                if (read == 0) break;
                count += read;
            }
            if (count > 64) throw new InvalidDataException("Refresh settings exceed the size limit.");
            try
            {
                using var document = JsonDocument.Parse(bytes.AsMemory(0, count));
                if (document.RootElement.ValueKind != JsonValueKind.Number || !document.RootElement.TryGetInt32(out var seconds) || !RefreshSettingsViewModel.IsSupported(seconds))
                    throw new InvalidDataException("Unsupported refresh interval.");
                return seconds;
            }
            catch (JsonException error) { throw new InvalidDataException("Invalid refresh settings.", error); }
        }
        finally { gate.Release(); }
    }

    /// <inheritdoc />
    public async Task SaveAsync(int seconds, CancellationToken cancellationToken = default)
    {
        if (!RefreshSettingsViewModel.IsSupported(seconds)) throw new ArgumentOutOfRangeException(nameof(seconds));
        await gate.WaitAsync(cancellationToken);
        string? temporary = null;
        try
        {
            var directory = Path.GetDirectoryName(path)!;
            Directory.CreateDirectory(directory);
            temporary = Path.Combine(directory, ".refresh-" + Guid.NewGuid().ToString("N") + ".tmp");
            await using (var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None, 64, FileOptions.Asynchronous | FileOptions.WriteThrough))
            {
                await stream.WriteAsync(Encoding.UTF8.GetBytes(seconds.ToString(CultureInfo.InvariantCulture)), cancellationToken);
                await stream.FlushAsync(cancellationToken);
                stream.Flush(true);
            }
            cancellationToken.ThrowIfCancellationRequested();
            File.Move(temporary, path, true);
            temporary = null;
        }
        finally
        {
            if (temporary is not null)
            {
                try { File.Delete(temporary); }
                catch (IOException) { }
                catch (UnauthorizedAccessException) { }
            }
            gate.Release();
        }
    }
}
