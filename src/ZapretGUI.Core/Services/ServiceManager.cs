using System.Diagnostics;
using System.ServiceProcess;
using Microsoft.Win32;
using ZapretGUI.Core.Models;
using ZapretGUI.Core.Strategies;

namespace ZapretGUI.Core.Services;

/// <summary>
/// Installs/removes/queries the "zapret" Windows service, mirroring service.bat's
/// :service_install / :service_remove / :service_status routines via sc.exe.
/// </summary>
public sealed class ServiceManager
{
    private const string ServiceName = "zapret";
    private const string RegistryKeyPath = @"System\CurrentControlSet\Services\zapret";
    private const string RegistryValueName = "zapret-discord-youtube";

    private static readonly string ScExePath =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "sc.exe");

    public ZapretStatus GetStatus(string binDir)
    {
        return new ZapretStatus
        {
            ServiceStatus = MapStatus(TryGetControllerStatus(ServiceName)),
            InstalledStrategyFileName = ReadInstalledStrategyName(),
            WinwsProcessRunning = ProcessRunner.IsAnyWinwsProcessRunning(),
            WinDivertSysPresent = Directory.Exists(binDir) && Directory.GetFiles(binDir, "*.sys").Length > 0
        };
    }

    public async Task InstallAsync(ResolvedStrategy strategy, string strategyFileName, CancellationToken ct = default)
    {
        await RemoveAsync(ct);

        var binPathValue = $"\"{strategy.ExecutablePath}\" {strategy.Arguments}";

        var createResult = await RunScAsync(
            ["create", ServiceName, "binPath=", binPathValue, "DisplayName=", "zapret", "start=", "auto"], ct);
        if (createResult.ExitCode != 0)
        {
            throw new InvalidOperationException($"sc create failed (exit {createResult.ExitCode}): {createResult.Output}");
        }

        await RunScAsync(["description", ServiceName, "Zapret DPI bypass software"], ct);

        var startResult = await RunScAsync(["start", ServiceName], ct);
        if (startResult.ExitCode != 0)
        {
            throw new InvalidOperationException($"sc start failed (exit {startResult.ExitCode}): {startResult.Output}");
        }

        WriteInstalledStrategyName(strategyFileName);
    }

    public async Task RemoveAsync(CancellationToken ct = default)
    {
        await TryRunScAsync(["stop", ServiceName], ct);
        await TryRunScAsync(["delete", ServiceName], ct);

        ProcessRunner.KillAllWinwsProcesses();

        await TryRunScAsync(["stop", "WinDivert"], ct);
        await TryRunScAsync(["delete", "WinDivert"], ct);
        await TryRunScAsync(["stop", "WinDivert14"], ct);
        await TryRunScAsync(["delete", "WinDivert14"], ct);
    }

    private static ServiceControllerStatus? TryGetControllerStatus(string serviceName)
    {
        try
        {
            using var controller = new ServiceController(serviceName);
            return controller.Status;
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    private static ServiceStatus MapStatus(ServiceControllerStatus? status) => status switch
    {
        null => ServiceStatus.NotInstalled,
        ServiceControllerStatus.Running => ServiceStatus.Running,
        ServiceControllerStatus.StartPending => ServiceStatus.StartPending,
        ServiceControllerStatus.StopPending => ServiceStatus.StopPending,
        _ => ServiceStatus.Stopped
    };

    private static void WriteInstalledStrategyName(string strategyFileName)
    {
        var nameWithoutExt = Path.GetFileNameWithoutExtension(strategyFileName);
        using var key = Registry.LocalMachine.CreateSubKey(RegistryKeyPath, writable: true);
        key?.SetValue(RegistryValueName, nameWithoutExt, RegistryValueKind.String);
    }

    private static string? ReadInstalledStrategyName()
    {
        using var key = Registry.LocalMachine.OpenSubKey(RegistryKeyPath, writable: false);
        return key?.GetValue(RegistryValueName) as string;
    }

    private static async Task TryRunScAsync(string[] args, CancellationToken ct)
    {
        try
        {
            await RunScAsync(args, ct);
        }
        catch
        {
            // best-effort cleanup, mirrors the original's "ignore failures" behavior
        }
    }

    private static async Task<(int ExitCode, string Output)> RunScAsync(string[] args, CancellationToken ct)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = ScExePath,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        foreach (var arg in args)
        {
            startInfo.ArgumentList.Add(arg);
        }

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Failed to start sc.exe");

        var stdoutTask = process.StandardOutput.ReadToEndAsync(ct);
        var stderrTask = process.StandardError.ReadToEndAsync(ct);
        await process.WaitForExitAsync(ct);

        var output = (await stdoutTask) + (await stderrTask);
        return (process.ExitCode, output);
    }
}
