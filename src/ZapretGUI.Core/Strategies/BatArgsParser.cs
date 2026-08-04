using System.Text;

namespace ZapretGUI.Core.Strategies;

/// <summary>
/// Extracts the winws.exe argument template from an upstream general*.bat strategy file
/// and substitutes the %BIN%/%LISTS%/%GameFilter*% tokens, mirroring the parsing done by
/// service.bat's :service_install routine but without batch's fragile tokenizing.
/// </summary>
public static class BatArgsParser
{
    private const string ExeMarker = "winws.exe\"";

    /// <summary>
    /// Reads a general*.bat file and returns the raw (un-substituted) argument string
    /// that follows the "...winws.exe" invocation, with line-continuation carets removed.
    /// </summary>
    public static string ExtractRawArgsTemplate(string batFilePath)
    {
        var lines = File.ReadAllLines(batFilePath);
        var sb = new StringBuilder();
        var capturing = false;

        foreach (var rawLine in lines)
        {
            var line = rawLine;

            if (!capturing)
            {
                var idx = line.IndexOf(ExeMarker, StringComparison.OrdinalIgnoreCase);
                if (idx < 0)
                {
                    continue;
                }

                capturing = true;
                line = line[(idx + ExeMarker.Length)..];
            }

            var trimmed = line.TrimEnd();
            var continues = trimmed.EndsWith('^');
            if (continues)
            {
                trimmed = trimmed[..^1].TrimEnd();
            }

            trimmed = trimmed.Trim();
            if (trimmed.Length > 0)
            {
                if (sb.Length > 0)
                {
                    sb.Append(' ');
                }

                sb.Append(trimmed);
            }

            if (!continues)
            {
                break;
            }
        }

        if (!capturing)
        {
            throw new InvalidDataException($"Could not find a winws.exe invocation in '{batFilePath}'.");
        }

        return sb.ToString();
    }

    public static string Substitute(
        string template,
        string binDir,
        string listsDir,
        string gameFilter,
        string gameFilterTcp,
        string gameFilterUdp)
    {
        return template
            .Replace("%BIN%", EnsureTrailingSlash(binDir), StringComparison.Ordinal)
            .Replace("%LISTS%", EnsureTrailingSlash(listsDir), StringComparison.Ordinal)
            .Replace("%GameFilterTCP%", gameFilterTcp, StringComparison.Ordinal)
            .Replace("%GameFilterUDP%", gameFilterUdp, StringComparison.Ordinal)
            .Replace("%GameFilter%", gameFilter, StringComparison.Ordinal);
    }

    private static string EnsureTrailingSlash(string path) =>
        path.EndsWith('\\') ? path : path + '\\';
}
