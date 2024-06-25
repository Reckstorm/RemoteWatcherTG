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
                res = context.GetValue("Rules") as string;
            }
        });
        return res;
    }

    public static async Task SetRules(string rules)
    {
        await Task.Run(() =>
        {
            var context = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\RemoteWatcher", true);
            context = CreateIfNotPresent(context);
            context.SetValue("Rules", rules);
            context.Close();
        });
    }

    private static RegistryKey CreateIfNotPresent(RegistryKey registryKey)
    {
        if (registryKey != null) return registryKey;
        registryKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE", true).CreateSubKey("RemoteWatcher");
        return registryKey;
    }

    // public static async void AddToStartup()
    // {
    //     await Task.Run(() =>
    //     {
    //         RegistryKey rk = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
    //         if (rk.GetValue(AppDomain.CurrentDomain.FriendlyName) == null)
    //             rk.SetValue(AppDomain.CurrentDomain.FriendlyName, $"\"{System.Reflection.Assembly.GetExecutingAssembly().Location}\"");
    //         rk.Close();
    //     });
    // }
}