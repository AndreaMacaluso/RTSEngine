namespace RTSEngine.Core.Events;

/// <summary>
/// Tick-ordered event bus.
/// <para>
/// <see cref="Publish"/> only enqueues. <see cref="Flush"/> is the single dispatch phase:
/// the simulation runner calls it at the end of the tick, after every system and before
/// AdvanceTick; it delivers pending events to subscribers in FIFO order, then clears the
/// queue. There is no second consumption timing.
/// </para>
/// <para>
/// Subscriber contract (determinism): subscribers observe only — they must never mutate
/// the world or any simulation state, and must not throw. Callbacks run inline in the
/// flushing tick (no threads, no wall-clock dependency), in subscribe order, so replaying
/// the same ticks produces the same callbacks. Subscribe/unsubscribe during a dispatch
/// takes effect at the next Flush.
/// </para>
/// </summary>
public sealed class EventBus
{
    private readonly Queue<GameEvent> _pending = new();
    private readonly List<Action<GameEvent>> _subscribers = [];
    private Action<GameEvent>[] _dispatchSnapshot = [];
    private bool _snapshotStale = true;

    public int PendingCount => _pending.Count;

    public void Subscribe(Action<GameEvent> subscriber)
    {
        _subscribers.Add(subscriber);
        _snapshotStale = true;
    }

    public void Unsubscribe(Action<GameEvent> subscriber)
    {
        if (_subscribers.Remove(subscriber))
        {
            _snapshotStale = true;
        }
    }

    public void Publish(GameEvent gameEvent)
    {
        _pending.Enqueue(gameEvent);
    }

    public void Flush()
    {
        if (_pending.Count == 0)
        {
            return;
        }

        if (_snapshotStale)
        {
            _dispatchSnapshot = [.. _subscribers];
            _snapshotStale = false;
        }

        while (_pending.Count > 0)
        {
            var gameEvent = _pending.Dequeue();

            foreach (var subscriber in _dispatchSnapshot)
            {
                subscriber(gameEvent);
            }
        }
    }
}
