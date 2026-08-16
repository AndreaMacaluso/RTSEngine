using RTSEngine.Core.Entities;
using RTSEngine.Core.Entities.Buildings;
using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Helpers;
using RTSEngine.Core.Players;
using RTSEngine.Core.State;

namespace RTSEngine.Core.AI.Planning;

public static class CombatPlanner
{
    public static Unit? FindNearestEnemy(
        GameWorld world,
        Player player,
        Unit unit)
    {
        return world.Entities
            .OfType<Unit>()
            .Where(u =>
                u.OwnerId != player.Id
                && !u.IsDead)
            .OrderBy(u =>
                WorldQueries.ChebyshevDistance(
                    unit.Position,
                    u.Position))
            .FirstOrDefault();
    }

    public static (Entity Entity, int OwnerId)? FindNearestEnemyEntity(
        GameWorld world,
        Player player,
        Unit unit)
    {
        var units = world.Entities
            .OfType<Unit>()
            .Where(u => u.OwnerId != player.Id && !u.IsDead)
            .Select(u => (Entity: (Entity)u, OwnerId: u.OwnerId));

        var buildings = world.Entities
            .OfType<Building>()
            .Where(b => b.OwnerId != player.Id && !b.IsDead)
            .Select(b => (Entity: (Entity)b, OwnerId: b.OwnerId));

        return units.Concat(buildings)
            .OrderBy(e => WorldQueries.ChebyshevDistance(
                unit.Position,
                e.Entity.Position))
            .FirstOrDefault();
    }

    public static Building? FindEnemyTownCenter(
        GameWorld world,
        Player player)
    {
        return world.Entities
            .OfType<Building>()
            .FirstOrDefault(b =>
                b.OwnerId != player.Id
                && b.Definition.Id == "town_center"
                && b.IsCompleted
                && !b.IsDead);
    }

    public static bool HasMilitaryUnits(
        GameWorld world,
        Player player)
    {
        return world.Entities
            .OfType<Unit>()
            .Any(u =>
                u.OwnerId == player.Id
                && !u.IsDead
                && u.Definition.CanAttack);
    }

    public static bool HasEnemies(
        GameWorld world,
        Player player)
    {
        var hasEnemyUnits = world.Entities
            .OfType<Unit>()
            .Any(u => u.OwnerId != player.Id && !u.IsDead);

        var hasEnemyBuildings = world.Entities
            .OfType<Building>()
            .Any(b => b.OwnerId != player.Id && !b.IsDead);

        return hasEnemyUnits || hasEnemyBuildings;
    }

    public static int CountIdleMilitary(
        GameWorld world,
        Player player)
    {
        return world.Entities
            .OfType<Unit>()
            .Count(u =>
                u.OwnerId == player.Id
                && !u.IsDead
                && u.Definition.CanAttack
                && u.CurrentTask == UnitTask.Idle);
    }
}
