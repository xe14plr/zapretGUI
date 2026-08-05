using ZapretGUI.Core.Models;

namespace ZapretGUI.Core.Services;

/// <summary>
/// Probes a small set of known Discord/YouTube endpoints (same grouping as upstream's
/// utils/targets.txt) to report whether each service is currently reachable. Used both for
/// the Dashboard's live status chips and to score candidate strategies during auto-select.
/// </summary>
public sealed class ServiceHealthCheckService
{
    private static readonly IReadOnlyDictionary<string, string[]> Groups = new Dictionary<string, string[]>
    {
        ["Discord"] = ["https://discord.com", "https://gateway.discord.gg"],
        ["YouTube"] = ["https://www.youtube.com", "https://i.ytimg.com"]
    };

    private readonly HttpClient _httpClient;

    public ServiceHealthCheckService(HttpClient? httpClient = null)
    {
        _httpClient = httpClient ?? new HttpClient { Timeout = TimeSpan.FromSeconds(4) };
    }

    public async Task<List<ServiceHealth>> CheckAllAsync(CancellationToken ct = default)
    {
        var results = await Task.WhenAll(Groups.Select(g => CheckGroupAsync(g.Key, g.Value, ct)));
        return results.ToList();
    }

    private async Task<ServiceHealth> CheckGroupAsync(string name, string[] urls, CancellationToken ct)
    {
        foreach (var url in urls)
        {
            try
            {
                using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct);
                return new ServiceHealth { GroupName = name, IsReachable = true };
            }
            catch
            {
                // DPI blocking usually shows up as a reset/timeout rather than a valid HTTP
                // response (even an error status code still proves the handshake got through),
                // so only an exception here counts as "unreachable" - try the next host.
            }
        }

        return new ServiceHealth { GroupName = name, IsReachable = false };
    }
}
