using System.IO;

namespace ZapretGUI.App.Services;

public static class AppPaths
{
    public static string BaseDir { get; } = AppContext.BaseDirectory;
    public static string BinDir { get; } = Path.Combine(BaseDir, "bin");
    public static string ListsDir { get; } = Path.Combine(BaseDir, "lists");
    public static string StrategiesDir { get; } = Path.Combine(BaseDir, "Strategies");
    public static string ConfigPath { get; } = Path.Combine(BaseDir, "config.json");
}
