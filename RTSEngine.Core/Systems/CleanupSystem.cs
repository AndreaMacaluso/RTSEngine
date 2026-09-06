using RTSEngine.Core.Actions;
using RTSEngine.Core.Entities;
using RTSEngine.Core.Entities.Buildings;
using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Helpers;
using RTSEngine.Core.Players;
using RTSEngine.Core.State;

namespace RTSEngine.Core.Systems;

public static class CleanupSystem
{
    public static void Update(GameWorld world)
    {
        TickDecay(world);
        RemoveDeadUnits(world);
        RemoveDeadBuildings(world);
        CleanupResources(world);
    }

    private static void TickDecay(GameWorld world)
    {
        foreach (var unit in world.Entities.Units.Values)
        {
            if (unit.CurrentTask == EntityState.Decaying)
            {
                unit.DecayTicksRemaining--;

                if (unit.DecayTicksRemaining <= 0)
                {
                    unit.CurrentTask = EntityState.Dead;
                }
            }
        }

        foreach (var building in world.Entities.Buildings.Values)
        {
            if (building.CurrentTask == EntityState.Decaying)
            {
                building.DecayTicksRemaining--;

                if (building.DecayTicksRemaining <= 0)
                {
                    building.CurrentTask = EntityState.Dead;
                }
            }
        }
    }

    private static void RemoveDeadUnits(GameWorld world)
    {
        var removeIds = new List<int>();

        foreach (var unit in world.Entities.Units.Values)
        {
            if (unit.CurrentTask == EntityState.Dead)
            {
                removeIds.Add(unit.Id);
            }
        }

        foreach (var unitId in removeIds)
        {
            var unit = world.Entities.GetUnitById(unitId);

            if (unit == null)
            {
                continue;
            }

            var owner = world.GetPlayerById(unit.OwnerId);

            if (owner is Player player)
            {
                world.Entities.Remove(unit, player);
            }
        }
    }

    private static void RemoveDeadBuildings(GameWorld world)
    {
        var removeIds = new List<int>();

        foreach (var building in world.Entities.Buildings.Values)
        {
            if (building.CurrentTask == EntityState.Dead)
            {
                ReleaseBuildingPopulation(world, building);
                removeIds.Add(building.Id);
            }
        }

        foreach (var buildingId in removeIds)
        {
            var building = world.Entities.GetBuildingById(buildingId);

            if (building == null)
            {
                continue;
            }

            var owner = world.GetPlayerById(building.OwnerId);

            if (owner is Player player)
            {
                world.Entities.Remove(building, player);
            }
        }
    }

    private static void CleanupResources(GameWorld world)
    {
        foreach (var resource in WorldQueries.FindDepletedResources(world))
        {
            world.Entities.Remove(resource);
        }
    }

    private static void ReleaseBuildingPopulation(
        GameWorld world,
        Building building)
    {
        if (building.Definition.PopulationBonus <= 0)
        {
            return;
        }

        var owner = world.GetPlayerById(building.OwnerId);

        if (owner is not Player player)
        {
            return;
        }

        PopulationActions.DecreaseCap(
            player,
            building.Definition.PopulationBonus);
    }
}
