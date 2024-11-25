using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using Domain;

public class Blocker
{
    private static Blocker _instance = null;
    private List<Rule> _RuleList { get; set; }
    private static object _locker { get; set; } = new object();
    private bool _running;
    public bool running
    {
        get { lock (_locker) return _running; }
        set { lock (_locker) _running = value; }
    }

    private bool _unblock = false;
    public bool unblock
    {
        get { lock (_locker) return _unblock; }
        set { lock (_locker) _unblock = value; }
    }

    private Blocker()
    {
    }

    public static Blocker GetInstance(List<Rule> Rules)
    {
        lock (_locker)
        {
            if (_instance != null) return _instance;
            _instance = new Blocker
            {
                _RuleList = Rules
            };
            CheckIfBGRunning();
            return _instance;
        }
    }

    public static Blocker GetInstance()
    {
        lock (_locker)
        {
            return _instance;
        }
    }

    private static void CheckIfBGRunning()
    {
        if (_instance._RuleList == null) return;
        Process temp = Process.GetProcesses().FirstOrDefault(p => p.ProcessName.Equals(AppDomain.CurrentDomain.FriendlyName));
        if (temp != null && temp.Id != Process.GetCurrentProcess().Id) temp.Kill();
    }

    public async Task StopBlock()
    {
        await Task.Run(() =>
        {
            if (running)
            {
                running = false;
            }
        });
    }

    public async Task Unblock()
    {
        await Task.Run(() =>
        {
            if (!unblock)
            {
                unblock = true;
            }
        });
    }

    public async Task Block()
    {
        await Task.Run(() =>
        {
            if (unblock)
            {
                unblock = false;
            }
        });
    }

    public void RunBlock()
    {
        if (!running)
        {
            running = true;
            RestartIfRulesChanged();
            Task.Run(async () =>
            {
                while (running)
                {
                    TimeOnly now = TimeOnly.Parse(DateTime.Now.ToLongTimeString());
                    var processes = Process.GetProcesses().ToList();
                    foreach (Process process in processes)
                    {
                        if (now.Equals(TimeOnly.Parse("00:00:00"))) await Block();
                        foreach (Rule p in _RuleList)
                        {
                            if (p.ProcessName.Equals(AppDomain.CurrentDomain.FriendlyName)) continue;
                            if (p.ProcessName.Equals(process.ProcessName) && CheckTime(p, now))
                            {
                                if (unblock && now <= p.BlockStartTime) continue;
                                foreach (Process temp in Process.GetProcessesByName(p.ProcessName))
                                {
                                    temp.Kill();
                                }
                                await WriteLogs(p.ProcessName);
                            }
                        }
                    }
                    Thread.Sleep(200);
                }
            });
        }
    }

    private bool CheckTime(Rule p, TimeOnly now) => (now <= p.BlockEndTime && now >= p.BlockStartTime) || (now >= p.BlockStartTime && p.BlockStartTime >= p.BlockEndTime) || (now <= p.BlockEndTime && p.BlockEndTime <= p.BlockStartTime);

    private async Task RestartBlocker()
    {
        await Task.Run(async () =>
        {
            await StopBlock();
            RunBlock();
        });
    }

    private async void RestartIfRulesChanged()
    {
        var rules = await RegistryAgent.GetRules();
        await Task.Run(async () =>
        {
            while (running)
            {
                var newRules = await RegistryAgent.GetRules();
                if (newRules != rules)
                {
                    rules = newRules;
                    _RuleList = JsonSerializer.Deserialize<List<Rule>>(rules);
                    await RestartBlocker();
                }
                Thread.Sleep(200);
            }
        });
    }

    private async Task WriteLogs(string processName)
    {
        if (!Directory.Exists($"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\Logs"))
            Directory.CreateDirectory($"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\Logs");
        var path = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\Logs\\{DateTime.Now.ToShortDateString()}.txt";
        await File.AppendAllTextAsync(path, $"Killed: {processName} at {DateTime.Now.ToLongTimeString()}\n");
    }
}