using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FlowNRW.Core.Diagnostics;

/// <summary>Optional local diagnostic protocol that can be mailed for support purposes.</summary>
public static class AppLog
{
    private const long MaximumBytes = 512 * 1024;
    private static readonly object Gate = new();

    /// <summary>Directory holding the log and its persisted enable flag.</summary>
    /// <value>Absolute directory path.</value>
    public static string DirectoryPath { get; }
    /// <summary>Current log file.</summary>
    /// <value>Absolute file path.</value>
    public static string FilePath { get; }
    private static string SettingsPath { get; }

    static AppLog()
    {
        DirectoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FlowNRW");
        FilePath = Path.Combine(DirectoryPath, "flownrw-diagnostic.log");
        SettingsPath = Path.Combine(DirectoryPath, "diagnostics.json");
    }

    /// <summary>Whether writes are appended to the log file.</summary>
    public static bool Enabled { get; private set; }

    /// <summary>Restores the persisted enable flag once at startup.</summary>
    public static void LoadEnabled()
    {
        try
        {
            if (File.Exists(SettingsPath)
                && JsonSerializer.Deserialize<Settings>(File.ReadAllText(SettingsPath)) is { } stored)
                Enabled = stored.Enabled;
        }
        catch (Exception) { Enabled = false; }
    }

    /// <summary>Persists the flag and announces the state change in the log itself.</summary>
    /// <param name="enabled">Requested state.</param>
    public static void SetEnabled(bool enabled)
    {
        if (enabled) { Enabled = true; Write("diagnostics", "Protokollierung aktiviert"); }
        else if (Enabled) Write("diagnostics", "Protokollierung deaktiviert");
        Enabled = enabled;
        lock (Gate)
        {
            try
            {
                System.IO.Directory.CreateDirectory(DirectoryPath);
                File.WriteAllText(SettingsPath, JsonSerializer.Serialize(new Settings { Enabled = enabled }));
            }
            catch (Exception) { }
        }
    }

    /// <summary>Appends a timestamped line while the file stays below the cap.</summary>
    /// <param name="category">Short subsystem name.</param>
    /// <param name="message">Technical message without secrets or coordinates.</param>
    public static void Write(string category, string message)
    {
        if (!Enabled) return;
        var line = DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss.fff zzz", CultureInfo.InvariantCulture) + " [" + category + "] " + message;
        lock (Gate)
        {
            try
            {
                System.IO.Directory.CreateDirectory(DirectoryPath);
                var info = new FileInfo(FilePath);
                if (info.Exists && info.Length > MaximumBytes)
                    File.WriteAllText(FilePath, string.Empty);
                File.AppendAllText(FilePath, line + Environment.NewLine);
            }
            catch (Exception) { }
        }
    }

    /// <summary>Reads the complete current protocol for the mail attachment.</summary>
    /// <returns>Log content or an explanatory placeholder.</returns>
    public static string ReadAll()
    {
        lock (Gate)
        {
            try
            {
                return File.Exists(FilePath) ? File.ReadAllText(FilePath) : "Protokoll ist leer.";
            }
            catch (Exception) { return "Protokoll konnte nicht gelesen werden."; }
        }
    }

    private sealed class Settings
    {
        [JsonPropertyName("enabled")] public bool Enabled { get; set; }
    }
}
