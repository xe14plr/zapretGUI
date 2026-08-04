using System.Diagnostics;
using ZapretGUI.Core.Strategies;

namespace ZapretGUI.Core.Services;

/// <summary>
/// Runs winws.exe as a plain foreground process (the "general*.bat run manually" path),
/// as opposed to installing it as a Windows service.
/// </summary>
public sealed class ProcessRunner
{
    private Process? _process;

    public bool IsRunning => _process is { HasExited: false };

    public void Start(ResolvedStrategy strategy)
    {
        Stop();

        var startInfo = new ProcessStartInfo
        {
            FileName = strategy.ExecutablePath,
            Arguments = strategy.Arguments,
            WorkingDirectory = Path.GetDirectoryName(strategy.ExecutablePath),
            UseShellExecute = false,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden
        };

        _process = Process.Start(startInfo);
    }

    public void Stop()
    {
        if (_process is { HasExited: false })
        {
            _process.Kill(entireProcessTree: true);
            _process.WaitForExit(5000);
        }

        _process?.Dispose();
        _process = null;
    }

    /// <summary>
    /// Detects a winws.exe running outside of this app's own tracked process handle
    /// (e.g. started via the original general*.bat, or a previous app session).
    /// </summary>
    public static bool IsAnyWinwsProcessRunning() =>
        Process.GetProcessesByName("winws").Length > 0;

    public static void KillAllWinwsProcesses()
    {
        foreach (var proc in Process.GetProcessesByName("winws"))
        {
            try
            {
                proc.Kill(entireProcessTree: true);
                proc.WaitForExit(5000);
            }
            catch
            {
                // best-effort cleanup
            }
            finally
            {
                proc.Dispose();
            }
        }
    }
}
