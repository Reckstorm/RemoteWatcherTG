namespace Application.RProcesses
{
    public class RProcessDTO
    {
        public TimeOnly StartTime { get; set; } = TimeOnly.MaxValue;
        public TimeOnly EndTime { get; set; } = TimeOnly.MaxValue;
    }
}