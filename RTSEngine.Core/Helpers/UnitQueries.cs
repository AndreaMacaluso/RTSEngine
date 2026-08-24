using RTSEngine.Core.Entities;
using RTSEngine.Core.Entities.Buildings;
using RTSEngine.Core.Entities.Resources;
using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Players;
using RTSEngine.Core.State;

namespace RTSEngine.Core.Helpers;

public static class UnitQueries
{
    public static IEnumerable<Unit> FindIdleVillagers(
        GameWorld world,
        Player player)
    {
        return world.Entities.GetUnits(player)
            .Where(unit => unit.CurrentTask == UnitTask.Idle);
    }

    public static int CountUnits(
        GameWorld world,
        Player player,
        string unitId)
    {
        return world.Entities.GetUnits(player)
            .Count(u =>
                !u.IsDead &&
                u.Definition.Id == unitId);
    }

    public static int CountGatherers(
        GameWorld world,
        Player player,
        ResourceType resourceType)
    {
        return world.Entities.GetUnits(player)
            .Count(u =>
                u.CurrentTask == UnitTask.Gathering &&
                u.Gather.CarriedResource == resourceType);
    }

    public static List<Unit> FindBuildersForBuilding(
        GameWorld world,
        Building building)
    {
        return world.Entities.Units.Values
            .Where(u =>
                u.Build.BuildingId == building.Id
                && u.Definition.CanBuild)
            .ToList();
    }

    public static List<Unit> FindDeadUnits(GameWorld world)
    {
        return world.Entities.Units.Values
            .Where(u => u.IsDead)
            .ToList();
    }

    public static List<Unit> FindIdleMilitary(
        GameWorld world,
        Player player)
    {
        return world.Entities.GetUnits(player)
            .Where(u =>
                !u.IsDead
                && u.Definition.CanAttack
                && u.CurrentTask == UnitTask.Idle)
            .ToList();
    }
}
