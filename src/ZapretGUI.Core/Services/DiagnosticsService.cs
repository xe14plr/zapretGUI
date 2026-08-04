using System.Diagnostics;
using System.ServiceProcess;
using Microsoft.Win32;
using ZapretGUI.Core.Models;

namespace ZapretGUI.Core.Services;

/// <summary>
/// Reimplements the checks from service.bat's :service_diagnostics: Base Filtering Engine,
/// system proxy, TCP timestamps, known conflicting software (Adguard/Killer/Check Point/
/// SmartByte/Intel/VPN/other bypasses), DoH configuration, hosts file conflicts, and a
/// stray-WinDivert-service check.
/// </summary>
public sealed class DiagnosticsService
{
    private static readonly string[] ConflictingBypassServiceNames =
        ["GoodbyeDPI", "discordfix_zapret", "winws1", "winws2"];

    public List<DiagnosticResult> Run(string binDir)
    {
        return
        [
            CheckBaseFilteringEngine(),
            CheckSystemProxy(),
            CheckTcpTimestamps(),
            CheckAdguard(),
            CheckServiceGroup("Killer", "Killer", "https://github.com/Flowseal/zapret-discord-youtube/issues/2512#issuecomment-2821119513"),
            CheckIntelConnectivity(),
            CheckCheckPoint(),
            CheckServiceGroup("SmartByte", "SmartByte", null),
            CheckWinDivertSysFile(binDir),
            CheckServiceGroup("VPN", "VPN", null),
            CheckDoh(),
            CheckHostsFile(),
            CheckStrayWinDivert(),
            CheckConflictingBypasses()
        ];
    }

    public void EnableTcpTimestamps() => RunCommand("netsh", "interface tcp set global timestamps=enabled");

    public async Task RemoveStrayWinDivertAsync(CancellationToken ct = default)
    {
        await RunScAsync(["stop", "WinDivert"], ct);
        await RunScAsync(["delete", "WinDivert"], ct);
        await RunScAsync(["stop", "WinDivert14"], ct);
        await RunScAsync(["delete", "WinDivert14"], ct);
    }

    public async Task RemoveConflictingBypassesAsync(CancellationToken ct = default)
    {
        foreach (var name in ConflictingBypassServiceNames)
        {
            if (!ServiceExists(name)) continue;
            await RunScAsync(["stop", name], ct);
            await RunScAsync(["delete", name], ct);
        }
    }

    public List<string> ClearDiscordCache()
    {
        var log = new List<string>();
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        ClearDiscordVariant("Discord", "Discord.exe", Path.Combine(appData, "discord"), log);
        ClearDiscordVariant("Discord PTB", "DiscordPTB.exe", Path.Combine(appData, "discordptb"), log);

        if (log.Count == 0)
        {
            log.Add("Discord и Discord PTB не найдены на этом компьютере.");
        }

        return log;
    }

    private static void ClearDiscordVariant(string label, string processFileName, string cacheDir, List<string> log)
    {
        if (!Directory.Exists(cacheDir))
        {
            return;
        }

        var exeName = Path.GetFileNameWithoutExtension(processFileName);
        foreach (var proc in Process.GetProcessesByName(exeName))
        {
            try
            {
                proc.Kill(entireProcessTree: true);
                proc.WaitForExit(5000);
                log.Add($"{label}: процесс был закрыт.");
            }
            catch
            {
                log.Add($"{label}: не удалось закрыть процесс.");
            }
            finally
            {
                proc.Dispose();
            }
        }

        foreach (var sub in new[] { "Cache", "Code Cache", "GPUCache" })
        {
            var dir = Path.Combine(cacheDir, sub);
            if (!Directory.Exists(dir))
            {
                continue;
            }

            try
            {
                Directory.Delete(dir, recursive: true);
                log.Add($"{label}: папка {sub} удалена.");
            }
            catch
            {
                log.Add($"{label}: не удалось удалить папку {sub}.");
            }
        }
    }

    private static DiagnosticResult CheckBaseFilteringEngine()
    {
        var running = IsServiceRunning("BFE");
        return new DiagnosticResult
        {
            Category = "Base Filtering Engine",
            Severity = running ? DiagnosticSeverity.Ok : DiagnosticSeverity.Error,
            Message = running
                ? "Служба BFE работает."
                : "Base Filtering Engine не запущена. Эта служба обязательна для работы zapret."
        };
    }

    private static DiagnosticResult CheckSystemProxy()
    {
        using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Internet Settings");
        var enabled = key?.GetValue("ProxyEnable") is int i && i == 1;

        if (!enabled)
        {
            return new DiagnosticResult { Category = "Системный прокси", Severity = DiagnosticSeverity.Ok, Message = "Прокси не используется." };
        }

        var server = key?.GetValue("ProxyServer") as string ?? "неизвестно";
        return new DiagnosticResult
        {
            Category = "Системный прокси",
            Severity = DiagnosticSeverity.Warning,
            Message = $"Включён системный прокси ({server}). Убедитесь, что он корректен, либо отключите его."
        };
    }

    private static DiagnosticResult CheckTcpTimestamps()
    {
        var output = RunCommand("netsh", "interface tcp show global");
        var timestampsLine = output
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault(l => l.Contains("timestamps", StringComparison.OrdinalIgnoreCase));

        var enabled = timestampsLine is not null && timestampsLine.Contains("enabled", StringComparison.OrdinalIgnoreCase);

        return new DiagnosticResult
        {
            Category = "TCP Timestamps",
            Severity = enabled ? DiagnosticSeverity.Ok : DiagnosticSeverity.Warning,
            Message = enabled ? "TCP timestamps включены." : "TCP timestamps выключены."
        };
    }

    private static DiagnosticResult CheckAdguard()
    {
        var running = Process.GetProcessesByName("AdguardSvc").Length > 0;
        return new DiagnosticResult
        {
            Category = "Adguard",
            Severity = running ? DiagnosticSeverity.Error : DiagnosticSeverity.Ok,
            Message = running ? "Обнаружен процесс Adguard. Он может вызывать проблемы с Discord." : "Adguard не обнаружен.",
            HelpUrl = running ? "https://github.com/Flowseal/zapret-discord-youtube/issues/417" : null
        };
    }

    private static DiagnosticResult CheckIntelConnectivity()
    {
        var found = FindServices((_, displayName) =>
            Contains(displayName, "Intel") && Contains(displayName, "Connectivity") && Contains(displayName, "Network"));

        return new DiagnosticResult
        {
            Category = "Intel Connectivity",
            Severity = found.Count > 0 ? DiagnosticSeverity.Error : DiagnosticSeverity.Ok,
            Message = found.Count > 0
                ? "Обнаружена служба Intel Connectivity Network Service. Она конфликтует с zapret."
                : "Intel Connectivity Network Service не обнаружена.",
            HelpUrl = found.Count > 0 ? "https://github.com/ValdikSS/GoodbyeDPI/issues/541#issuecomment-2661670982" : null
        };
    }

    private static DiagnosticResult CheckCheckPoint()
    {
        var found = FindServices((name, displayName) =>
            Contains(name, "TracSrvWrapper") || Contains(displayName, "TracSrvWrapper") ||
            Contains(name, "EPWD") || Contains(displayName, "EPWD"));

        return new DiagnosticResult
        {
            Category = "Check Point",
            Severity = found.Count > 0 ? DiagnosticSeverity.Error : DiagnosticSeverity.Ok,
            Message = found.Count > 0
                ? "Обнаружены службы Check Point. Check Point конфликтует с zapret — попробуйте удалить его."
                : "Check Point не обнаружен."
        };
    }

    private static DiagnosticResult CheckServiceGroup(string category, string substring, string? helpUrl)
    {
        var found = FindServices((name, displayName) => Contains(name, substring) || Contains(displayName, substring));

        return new DiagnosticResult
        {
            Category = category,
            Severity = found.Count > 0 ? (category == "VPN" ? DiagnosticSeverity.Warning : DiagnosticSeverity.Error) : DiagnosticSeverity.Ok,
            Message = found.Count > 0
                ? $"Обнаружены службы: {string.Join(", ", found)}."
                : $"{category} не обнаружен(ы).",
            HelpUrl = found.Count > 0 ? helpUrl : null
        };
    }

    private static DiagnosticResult CheckWinDivertSysFile(string binDir)
    {
        var present = Directory.Exists(binDir) && Directory.GetFiles(binDir, "*.sys").Length > 0;
        return new DiagnosticResult
        {
            Category = "WinDivert64.sys",
            Severity = present ? DiagnosticSeverity.Ok : DiagnosticSeverity.Error,
            Message = present ? "Файл драйвера найден." : "Файл WinDivert64.sys не найден в папке bin."
        };
    }

    private static DiagnosticResult CheckDoh()
    {
        var found = false;

        using var baseKey = Registry.LocalMachine.OpenSubKey(
            @"System\CurrentControlSet\Services\Dnscache\InterfaceSpecificParameters");

        if (baseKey is not null)
        {
            foreach (var ifaceName in baseKey.GetSubKeyNames())
            {
                using var ifaceKey = baseKey.OpenSubKey(ifaceName);
                if (ifaceKey is null) continue;

                foreach (var valueName in ifaceKey.GetValueNames())
                {
                    if (valueName.Contains("DohFlags", StringComparison.OrdinalIgnoreCase) &&
                        ifaceKey.GetValue(valueName) is int flags && flags > 0)
                    {
                        found = true;
                    }
                }
            }
        }

        return new DiagnosticResult
        {
            Category = "Secure DNS (DoH)",
            Severity = found ? DiagnosticSeverity.Ok : DiagnosticSeverity.Warning,
            Message = found
                ? "Обнаружена настроенная защищённая DNS (DoH)."
                : "Не обнаружено настроенной защищённой DNS. Настройте её в браузере или в параметрах Windows 11."
        };
    }

    private static DiagnosticResult CheckHostsFile()
    {
        var hostsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "drivers", "etc", "hosts");
        if (!File.Exists(hostsPath))
        {
            return new DiagnosticResult { Category = "Hosts", Severity = DiagnosticSeverity.Ok, Message = "Файл hosts не найден." };
        }

        var content = File.ReadAllText(hostsPath);
        var found = content.Contains("youtube.com", StringComparison.OrdinalIgnoreCase) ||
                    content.Contains("youtu.be", StringComparison.OrdinalIgnoreCase);

        return new DiagnosticResult
        {
            Category = "Hosts",
            Severity = found ? DiagnosticSeverity.Warning : DiagnosticSeverity.Ok,
            Message = found
                ? "В hosts есть записи для youtube.com/youtu.be — это может мешать доступу к YouTube."
                : "Конфликтующих записей в hosts не найдено."
        };
    }

    private static DiagnosticResult CheckStrayWinDivert()
    {
        var winwsRunning = ProcessRunner.IsAnyWinwsProcessRunning();
        var windivertActive = IsServiceRunning("WinDivert") || IsServicePending("WinDivert");
        var stray = !winwsRunning && windivertActive;

        return new DiagnosticResult
        {
            Category = "WinDivert",
            Severity = stray ? DiagnosticSeverity.Warning : DiagnosticSeverity.Ok,
            Message = stray
                ? "winws.exe не запущен, но служба WinDivert всё ещё активна."
                : "Конфликтов WinDivert не обнаружено."
        };
    }

    private static DiagnosticResult CheckConflictingBypasses()
    {
        var found = ConflictingBypassServiceNames.Where(ServiceExists).ToList();

        return new DiagnosticResult
        {
            Category = "Другие DPI-обходы",
            Severity = found.Count > 0 ? DiagnosticSeverity.Error : DiagnosticSeverity.Ok,
            Message = found.Count > 0
                ? $"Обнаружены конфликтующие службы обхода: {string.Join(", ", found)}."
                : "Конфликтующих служб обхода не найдено."
        };
    }

    private static bool Contains(string source, string term) =>
        source.Contains(term, StringComparison.OrdinalIgnoreCase);

    private static List<string> FindServices(Func<string, string, bool> predicate)
    {
        var matches = new List<string>();
        foreach (var controller in ServiceController.GetServices())
        {
            if (predicate(controller.ServiceName, controller.DisplayName))
            {
                matches.Add(controller.DisplayName);
            }

            controller.Dispose();
        }

        return matches;
    }

    private static bool ServiceExists(string exactName)
    {
        try
        {
            using var controller = new ServiceController(exactName);
            _ = controller.Status;
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool IsServiceRunning(string name) => TryGetStatus(name) == ServiceControllerStatus.Running;

    private static bool IsServicePending(string name) => TryGetStatus(name) == ServiceControllerStatus.StopPending;

    private static ServiceControllerStatus? TryGetStatus(string name)
    {
        try
        {
            using var controller = new ServiceController(name);
            return controller.Status;
        }
        catch
        {
            return null;
        }
    }

    private static string RunCommand(string fileName, string arguments)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using var process = Process.Start(startInfo);
            if (process is null) return string.Empty;

            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit(5000);
            return output;
        }
        catch
        {
            return string.Empty;
        }
    }

    private static async Task RunScAsync(string[] args, CancellationToken ct)
    {
        try
        {
            var scPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "sc.exe");
            var startInfo = new ProcessStartInfo
            {
                FileName = scPath,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            foreach (var arg in args)
            {
                startInfo.ArgumentList.Add(arg);
            }

            using var process = Process.Start(startInfo);
            if (process is null) return;

            await process.WaitForExitAsync(ct);
        }
        catch
        {
            // best-effort
        }
    }
}
