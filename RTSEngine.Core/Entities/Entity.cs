using RTSEngine.Core.Map.Runtime;

namespace RTSEngine.Core.Entities;

public abstract class Entity
{
    public int Id { get; init; }

    public GridPosition Position { get; set; }
}