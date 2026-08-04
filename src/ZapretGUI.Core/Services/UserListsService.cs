namespace ZapretGUI.Core.Services;

/// <summary>
/// Ensures the user-editable list files exist with sensible defaults, mirroring
/// service.bat's :load_user_lists.
/// </summary>
public static class UserListsService
{
    public static void EnsureDefaults(string listsDir)
    {
        Directory.CreateDirectory(listsDir);

        EnsureFile(Path.Combine(listsDir, "ipset-exclude-user.txt"), "203.0.113.113/32" + Environment.NewLine);
        EnsureFile(
            Path.Combine(listsDir, "list-general-user.txt"),
            "# Never leave this file empty" + Environment.NewLine + "domain.example.abc" + Environment.NewLine);
        EnsureFile(Path.Combine(listsDir, "list-exclude-user.txt"), "domain.example.abc" + Environment.NewLine);
    }

    private static void EnsureFile(string path, string defaultContent)
    {
        if (!File.Exists(path))
        {
            File.WriteAllText(path, defaultContent);
        }
    }
}
