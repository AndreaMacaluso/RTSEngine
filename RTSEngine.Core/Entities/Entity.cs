using RTSEngine.Core.Map.Runtime;

namespace RTSEngine.Core.Entities;

public abstract class Entity
{
    public int Id { get; internal set; }

    public GridPosition Position { get; set; }

    public abstract bool IsBlocking { get; }

    public abstract bool IsDead { get; }

    public abstract void TakeDamage(int amount);
}