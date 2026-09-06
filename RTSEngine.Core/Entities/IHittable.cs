using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Entities.States;

namespace RTSEngine.Core.Entities;

public interface IHittable
{
    int Id { get; }
    int OwnerId { get; }
    GridPosition Position { get; }
    bool IsDead { get; }
    HealthComponent Health { get; }
    CombatState Combat { get; }
    void TakeDamage(int amount);
}
