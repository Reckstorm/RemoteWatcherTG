namespace Application.Processes;

public class ProcessDto
{
    public string ProcessName { get; set;}
    public int ProcessId { get; set;} = -1;
    public bool IsRunning { get; set; } = true;
}