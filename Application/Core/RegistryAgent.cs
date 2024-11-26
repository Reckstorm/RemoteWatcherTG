using Microsoft.Win32;

public static class RegistryAgent
{
    public static async Task<string> GetRules()
    {
        string res = string.Empty;
        await Task.Run(() =>
        {
            using (var context = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\RemoteWatcher", false))
            {
                if (context == null) return;
                var result = context.GetValue("Rules");
                if (result == null) return;
                res = result.ToString();
            }
        });
        return res;
    }

    public static async Task SetRules(string rules)
    {
        await SetRegistryValue(rules, "Rules");
    }

    public static async Task<string> GetUnblocker()
    {
        string res = string.Empty;
        await Task.Run(() =>
        {
            using (var context = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\RemoteWatcher", false))
            {
                if (context == null) return;
                var result = context.GetValue("Unblocker");
                if (result == null) return;
                res = result.ToString();
            }
        });
        return res;
    }

    public static async Task SetUnblocker(string unblocker)
    {
        await SetRegistryValue(unblocker, "Unblocker");
    }

    public static async Task SetRegistryValue(string value, string valueName)
    {
        await Task.Run(() =>
        {
            var context = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\RemoteWatcher", true);
            context = CreateIfNotPresent(context);
            context.SetValue(valueName, value);
            context.Close();
        });
    }

    private static RegistryKey CreateIfNotPresent(RegistryKey registryKey)
    {
        if (registryKey != null) return registryKey;
        registryKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE", true).CreateSubKey("RemoteWatcher");
        return registryKey;
    }
}