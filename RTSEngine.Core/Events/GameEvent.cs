using RTSEngine.Core.Map.Runtime;

namespace RTSEngine.Core.Events;

// TODO: int? and GridPosition? cause boxing on a struct. Consider using
// sentinel values (-1 for no owner, default Position for no position)
// or separating into typed event variants if this becomes a hot path.
public readonly struct GameEvent
{
    public int Tick { get; init; }
    public int Type { get; init; }
    public int EntityId { get; init; }
    public int? OwnerId { get; init; }
    public GridPosition? Position { get; init; }
    public string? Payload { get; init; }
}
