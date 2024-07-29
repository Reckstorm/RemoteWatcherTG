namespace Application.DTOs
{
    public class CommonDto
    {
        public string ProcessName { get; set; }
        public int ProcessId { get; set; } = -1;
        public TimeOnly StartTime { get; set; } = TimeOnly.MaxValue;
        public TimeOnly EndTime { get; set; } = TimeOnly.MaxValue;
    }
}