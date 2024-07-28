
namespace Domain;
public class Rule
{
    public string ProcessName { get; set; }
    private object locker { get; set; } = new object();

    private TimeOnly _blockStartTime;

    public TimeOnly BlockStartTime
    {
        get { lock (locker) return _blockStartTime; }
        set { lock (locker) _blockStartTime = value; }
    }

    private TimeOnly _blockEndTime;

    public TimeOnly BlockEndTime
    {
        get { lock (locker) return _blockEndTime; }
        set { lock (locker) _blockEndTime = value; }
    }


    public Rule()
    {
        BlockStartTime = TimeOnly.MaxValue;
        BlockEndTime = TimeOnly.MaxValue;
        ProcessName = string.Empty;
    }

    public Rule(string proc)
    {
        BlockStartTime = TimeOnly.MaxValue;
        BlockEndTime = TimeOnly.MaxValue;
        ProcessName = proc;
    }

    public Rule(string proc, TimeOnly blockStartTime, TimeOnly blockEndtTime)
    {
        ProcessName = proc;
        BlockStartTime = blockStartTime;
        BlockEndTime = blockEndtTime;
    }
}