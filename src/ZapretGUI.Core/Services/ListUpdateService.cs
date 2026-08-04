namespace ZapretGUI.Core.Services;

/// <summary>
/// Downloads the upstream ipset-all.txt from GitHub, mirroring service.bat's :ipset_update.
/// </summary>
public sealed class ListUpdateService
{
    private const string IpsetUrl =
        "https://raw.githubusercontent.com/Flowseal/zapret-discord-youtube/refs/heads/main/.service/ipset-service.txt";

    private readonly HttpClient _httpClient;

    public ListUpdateService(HttpClient? httpClient = null)
    {
        _httpClient = httpClient ?? new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
    }

    public async Task UpdateIpsetAllAsync(string ipsetAllPath, CancellationToken ct = default)
    {
        var content = await _httpClient.GetStringAsync(IpsetUrl, ct);
        await File.WriteAllTextAsync(ipsetAllPath, content, ct);

        var backupPath = ipsetAllPath + ".backup";
        if (File.Exists(backupPath))
        {
            File.Delete(backupPath);
        }
    }
}
