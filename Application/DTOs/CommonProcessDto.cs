namespace Application.DTOs
{
    public class CommonProcessDto
    {
        public string ProcessName { get; set; }
        public int ProcessId { get; set; } = -1;
        public bool IsRunning { get; set; } = true;
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
    }
}