using System.Text.RegularExpressions;
using ZapretGUI.Core.Models;

namespace ZapretGUI.Core.Strategies;

/// <summary>
/// Discovers the bundled general*.bat strategy files. Any file matching this pattern is
/// picked up automatically, so dropping in a new preset from upstream is enough to make
/// it show up in the app.
/// </summary>
public sealed class StrategyCatalog
{
    private readonly string _strategiesDir;

    public StrategyCatalog(string strategiesDir)
    {
        _strategiesDir = strategiesDir;
    }

    public IReadOnlyList<StrategyInfo> GetStrategies()
    {
        if (!Directory.Exists(_strategiesDir))
        {
            return Array.Empty<StrategyInfo>();
        }

        return Directory.GetFiles(_strategiesDir, "general*.bat")
            .OrderBy(NaturalSortKey, StringComparer.OrdinalIgnoreCase)
            .Select(path => new StrategyInfo
            {
                FileName = Path.GetFileName(path),
                FilePath = path,
                DisplayName = ToDisplayName(Path.GetFileNameWithoutExtension(path))
            })
            .ToList();
    }

    private static string ToDisplayName(string fileNameWithoutExt)
    {
        var start = fileNameWithoutExt.IndexOf('(');
        var end = fileNameWithoutExt.LastIndexOf(')');

        return start >= 0 && end > start
            ? fileNameWithoutExt[(start + 1)..end].Trim()
            : "Default";
    }

    private static string NaturalSortKey(string path) =>
        Regex.Replace(Path.GetFileName(path), @"\d+", m => m.Value.PadLeft(8, '0'));
}
