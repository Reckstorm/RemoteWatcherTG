using System.Diagnostics;
using Application.DTOs;

namespace Application.Core;

public static class ProcessAgent
{
    public static async Task<List<CommonDto>> GetProcesses()
    {
        var result = new List<CommonDto>();
        await Task.Run(() =>
        {
            foreach (var process in Process.GetProcesses())
            {
                if (!result.Any(p => p.ProcessName == process.ProcessName) && Environment.ProcessId != process.Id)
                {
                    result.Add(new CommonDto { ProcessName = process.ProcessName });
                }
            }
        });
        return result;
    }

    public static async Task<CommonDto> GetProcessDetails(string processName)
    {
        var result = new CommonDto();
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

    public static async Task<CommonDto> KillProcess(string processName)
    {
        var result = new CommonDto();
        await Task.Run(() =>
        {
            foreach (var process in Process.GetProcesses())
            {
                if (process.ProcessName == processName && process.Id != Environment.ProcessId)
                {
                    process.Kill();
                    result = new CommonDto { ProcessName = process.ProcessName, ProcessId = process.Id};
                }
            }
        });
        return result;
    }
}