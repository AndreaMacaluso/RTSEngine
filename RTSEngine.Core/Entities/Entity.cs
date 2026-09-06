using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.State;

namespace RTSEngine.Core.Entities;

public abstract class Entity
{
    public int Id { get; internal set; }

    public GridPosition Position { get; set; }

    public EntityState CurrentTask { get; set; } = EntityState.Idle;

    public int DecayTicksRemaining { get; set; }

    public abstract bool IsBlocking { get; }
}