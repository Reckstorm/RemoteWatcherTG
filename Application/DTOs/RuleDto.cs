namespace Application.DTOs
{
    public class RuleDto
    {
        public TimeOnly StartTime { get; set; } = TimeOnly.MaxValue;
        public TimeOnly EndTime { get; set; } = TimeOnly.MaxValue;
    }
}