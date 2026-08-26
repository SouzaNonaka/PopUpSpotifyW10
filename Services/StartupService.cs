using Microsoft.Win32;

namespace SpotifyMediaFlyout.Services;

public static class StartupService
{
    private const string AppName = "SpotifyMediaFlyout";
    private const string RunRegistryKey = @"Software\Microsoft\Windows\CurrentVersion\Run";

    public static bool IsStartupEnabled()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunRegistryKey, false);
            return key?.GetValue(AppName) != null;
        }
        catch
        {
            return false;
        }
    }

    public static void SetStartup(bool enable)
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunRegistryKey, true);
            if (key == null) return;

            if (enable)
            {
                var exePath = Environment.ProcessPath;
                if (!string.IsNullOrEmpty(exePath))
                {
                    key.SetValue(AppName, $"\"{exePath}\"");
                }
            }
            else
            {
                key.DeleteValue(AppName, false);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[StartupService] SetStartup error: {ex.Message}");
        }
    }
}
