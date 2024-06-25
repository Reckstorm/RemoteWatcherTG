using System.Diagnostics;
using Application.Processes;

namespace Application.Core;

public static class ProcessAgent
{
    public static async Task<List<ProcessDto>> GetProcesses()
    {
        var result = new List<ProcessDto>();
        await Task.Run(() =>
        {
            foreach (var process in Process.GetProcesses())
            {
                if (!result.Any(p => p.ProcessName == process.ProcessName) && Environment.ProcessId != process.Id)
                {
                    result.Add(new ProcessDto { ProcessName = process.ProcessName, ProcessId = process.Id });
                }
            }
        });
        return result;
    }

    public static async Task<ProcessDto> GetProcessDetails(string processName)
    {
        var result = new ProcessDto();
        await Task.Run(() =>
        {
            foreach (var process in Process.GetProcesses())
            {
                if (process.ProcessName == processName)
                {
                    result.ProcessName = process.ProcessName;
                    result.ProcessId = process.Id;
                }
            }
        });
        return result;
    }

    public static async Task<ProcessDto> KillProcess(string processName)
    {
        var result = new ProcessDto();
        await Task.Run(() =>
        {
            foreach (var process in Process.GetProcesses())
            {
                if (process.ProcessName == processName && process.Id != Environment.ProcessId)
                {
                    process.Kill();
                    result = new ProcessDto { ProcessName = process.ProcessName, ProcessId = process.Id, IsRunning = false };
                }
            }
        });
        return result;
    }
}