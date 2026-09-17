namespace RTSEngine.Core.Events;

public sealed class EventBus
{
    private readonly Queue<GameEvent> _pending = new();

    public int PendingCount => _pending.Count;

    public void Publish(GameEvent gameEvent)
    {
        _pending.Enqueue(gameEvent);
    }

    public IReadOnlyList<GameEvent> Flush()
    {
        var events = _pending.ToArray();
        _pending.Clear();
        return events;
    }
}
