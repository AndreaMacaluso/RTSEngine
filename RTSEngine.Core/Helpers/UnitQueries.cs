using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Players;
using RTSEngine.Core.State;

namespace RTSEngine.Core.Helpers;

public static class UnitQueries
{
    public static IEnumerable<Unit> FindIdleVillagers(
        GameWorld world,
        Player player)
    {
        return world.Entities
            .OfType<Unit>()
            .Where(unit =>
                unit.OwnerId == player.Id &&
                unit.CurrentTask == UnitTask.Idle);
    }
}
