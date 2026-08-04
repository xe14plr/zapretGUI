using System.Text.Json;
using ZapretGUI.Core.Models;

namespace ZapretGUI.Core.Services;

/// <summary>
/// Persists app-level settings (game filter mode, ipset filter mode, auto-update flag,
/// active strategy) to a local config.json next to the executable, instead of the
/// registry keys the original tooling scatters settings across.
/// </summary>
public sealed class SettingsStore
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private readonly string _configPath;

    public SettingsStore(string configPath)
    {
        _configPath = configPath;
    }

    public AppSettings Load()
    {
        if (!File.Exists(_configPath))
        {
            return new AppSettings();
        }

        try
        {
            var json = File.ReadAllText(_configPath);
            return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
        }
        catch
        {
            return new AppSettings();
        }
    }

    public void Save(AppSettings settings)
    {
        var json = JsonSerializer.Serialize(settings, JsonOptions);
        var dir = Path.GetDirectoryName(_configPath);
        if (!string.IsNullOrEmpty(dir))
        {
            Directory.CreateDirectory(dir);
        }

        File.WriteAllText(_configPath, json);
    }
}
