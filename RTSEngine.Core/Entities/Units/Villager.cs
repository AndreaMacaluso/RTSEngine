using RTSEngine.Core.Map.Runtime;
namespace RTSEngine.Core.Entities.Units;

public sealed class Villager : Unit
{
    public Villager(
        int ownerId,
        GridPosition position
        )
            : base(ownerId,position, movementSpeed: 0.25f)
        {
           
        }
}