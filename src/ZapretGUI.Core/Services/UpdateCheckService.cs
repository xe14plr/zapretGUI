namespace ZapretGUI.Core.Services;

public sealed class UpdateCheckResult
{
    public required string LocalVersion { get; init; }
    public string? LatestVersion { get; init; }
    public bool UpdateAvailable { get; init; }
    public string? Error { get; init; }
}

/// <summary>
/// Compares the bundled strategy-set version against the upstream repository's
/// .service/version.txt, mirroring service.bat's :service_check_updates.
/// </summary>
public sealed class UpdateCheckService
{
    public const string ReleasesUrl = "https://github.com/Flowseal/zapret-discord-youtube/releases/latest";

    private const string VersionUrl =
        "https://raw.githubusercontent.com/Flowseal/zapret-discord-youtube/main/.service/version.txt";

    private readonly HttpClient _httpClient;

    public UpdateCheckService(HttpClient? httpClient = null)
    {
        _httpClient = httpClient ?? new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
    }

    public async Task<UpdateCheckResult> CheckAsync(string localVersion, CancellationToken ct = default)
    {
        try
        {
            var latest = (await _httpClient.GetStringAsync(VersionUrl, ct)).Trim();
            return new UpdateCheckResult
            {
                LocalVersion = localVersion,
                LatestVersion = latest,
                UpdateAvailable = !string.Equals(latest, localVersion, StringComparison.OrdinalIgnoreCase)
            };
        }
        catch (Exception ex)
        {
            return new UpdateCheckResult { LocalVersion = localVersion, Error = ex.Message };
        }
    }
}
