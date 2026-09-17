namespace RTSEngine.Core.Helpers;

public sealed class SequentialIdGenerator
{
    private int _next;

    public SequentialIdGenerator(int startId = 1)
    {
        _next = startId;
    }

    public int Next() => _next++;

    public void Reset(int seed) => _next = seed;
}
