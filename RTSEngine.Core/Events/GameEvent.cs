using RTSEngine.Core.Map.Runtime;

namespace RTSEngine.Core.Events;

public readonly struct GameEvent
{
    public int Tick { get; init; }
    public EventType Type { get; init; }
    public int EntityId { get; init; }
    public int OwnerId { get; init; }
    public GridPosition Position { get; init; }
    public string? Payload { get; init; }
}
