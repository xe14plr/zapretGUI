using ZapretGUI.Core.Models;

namespace ZapretGUI.Core.Services;

/// <summary>
/// Cycles lists/ipset-all.txt between none/loaded/any, mirroring service.bat's
/// :ipset_switch_status / :ipset_switch. The three states form a strict cycle
/// (loaded -> none -> any -> loaded) because the "none" backup is only meaningful
/// when it was captured from the real "loaded" content.
/// </summary>
public sealed class IpsetFilterService
{
    private const string PlaceholderIp = "203.0.113.113/32";

    public IpsetFilterMode GetStatus(string ipsetAllPath)
    {
        if (!File.Exists(ipsetAllPath))
        {
            return IpsetFilterMode.Loaded;
        }

        var lines = File.ReadAllLines(ipsetAllPath).Where(l => !string.IsNullOrWhiteSpace(l)).ToArray();
        if (lines.Length == 0)
        {
            return IpsetFilterMode.Any;
        }

        return lines.Any(l => l.Trim() == PlaceholderIp) ? IpsetFilterMode.None : IpsetFilterMode.Loaded;
    }

    public IpsetFilterMode GetNextMode(IpsetFilterMode current) => current switch
    {
        IpsetFilterMode.Loaded => IpsetFilterMode.None,
        IpsetFilterMode.None => IpsetFilterMode.Any,
        IpsetFilterMode.Any => IpsetFilterMode.Loaded,
        _ => IpsetFilterMode.Loaded
    };

    public void SwitchTo(string ipsetAllPath, IpsetFilterMode targetMode)
    {
        var backupPath = ipsetAllPath + ".backup";
        var currentMode = GetStatus(ipsetAllPath);

        switch (targetMode)
        {
            case IpsetFilterMode.None:
                if (currentMode != IpsetFilterMode.Loaded)
                {
                    throw new InvalidOperationException("Переключение в режим 'none' возможно только из режима 'loaded'.");
                }

                File.Copy(ipsetAllPath, backupPath, overwrite: true);
                File.WriteAllText(ipsetAllPath, PlaceholderIp + Environment.NewLine);
                break;

            case IpsetFilterMode.Any:
                File.WriteAllText(ipsetAllPath, string.Empty);
                break;

            case IpsetFilterMode.Loaded:
                if (!File.Exists(backupPath))
                {
                    throw new InvalidOperationException("Резервная копия списка не найдена. Сначала обновите список IPSet.");
                }

                if (File.Exists(ipsetAllPath))
                {
                    File.Delete(ipsetAllPath);
                }

                File.Move(backupPath, ipsetAllPath);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(targetMode), targetMode, null);
        }
    }
}
