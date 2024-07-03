using System.Diagnostics;
using Application.DTOs;
using Application.Processes;
using Domain;

namespace Application.Core;

public static class ProcessAgent
{
    public static async Task<List<CommonProcessDto>> GetProcesses()
    {
        var result = new List<CommonProcessDto>();
        await Task.Run(() =>
        {
            foreach (var process in Process.GetProcesses())
            {
                if (!result.Any(p => p.ProcessName == process.ProcessName) && Environment.ProcessId != process.Id)
                {
                    result.Add(new CommonProcessDto { ProcessName = process.ProcessName });
                }
            }
        });
        return result;
    }

    public static async Task<CommonProcessDto> GetProcessDetails(string processName)
    {
        var result = new CommonProcessDto();
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

    public static async Task<CommonProcessDto> KillProcess(string processName)
    {
        var result = new CommonProcessDto();
        await Task.Run(() =>
        {
            foreach (var process in Process.GetProcesses())
            {
                if (process.ProcessName == processName && process.Id != Environment.ProcessId)
                {
                    process.Kill();
                    result = new CommonProcessDto { ProcessName = process.ProcessName, ProcessId = process.Id, IsRunning = false };
                }
            }
        });
        return result;
    }
}