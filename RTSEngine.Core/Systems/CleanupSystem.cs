using RTSEngine.Core.Actions;
using RTSEngine.Core.Entities;
using RTSEngine.Core.Entities.Buildings;
using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Events;
using RTSEngine.Core.Helpers;
using RTSEngine.Core.State;

namespace RTSEngine.Core.Systems;

public static class CleanupSystem
{
    public static void Update(RuntimeContext context)
    {
        TickDecay(context.World.Entities.Units);
        TickDecay(context.World.Entities.Buildings);
        RemoveDeadUnits(context);
        RemoveDeadBuildings(context);
        CleanupResources(context);
    }

    private static void TickDecay<T>(IReadOnlyList<T> entities) where T : Entity
    {
        foreach (var entity in entities)
        {
            if (entity.CurrentTask == EntityState.Decaying)
            {
                entity.DecayTicksRemaining--;

                if (entity.DecayTicksRemaining <= 0)
                {
                    entity.CurrentTask = EntityState.Dead;
                }
            }
        }
    }

    private static void RemoveDeadUnits(RuntimeContext context)
    {
        var world = context.World;
        var toRemove = new List<Unit>();

        foreach (var unit in world.Entities.Units)
        {
            if (unit.CurrentTask == EntityState.Dead)
            {
                toRemove.Add(unit);
            }
        }

        foreach (var unit in toRemove)
        {
            var player = world.GetPlayerById(unit.OwnerId);
            if (player != null)
            {
                world.Entities.Remove(unit, player);

                context.Events.Publish(new GameEvent
                {
                    Tick = world.CurrentTick,
                    Type = (int)EventType.UnitRemoved,
                    EntityId = unit.Id,
                    OwnerId = unit.OwnerId,
                    Position = unit.Position
                });
            }
        }
    }

    private static void RemoveDeadBuildings(RuntimeContext context)
    {
        var world = context.World;
        var toRemove = new List<Building>();

        foreach (var building in world.Entities.Buildings)
        {
            if (building.CurrentTask == EntityState.Dead)
            {
                ReleaseBuildingPopulation(world, building);
                toRemove.Add(building);
            }
        }

        foreach (var building in toRemove)
        {
            var player = world.GetPlayerById(building.OwnerId);
            if (player != null)
            {
                world.Entities.Remove(building, player);

                context.Events.Publish(new GameEvent
                {
                    Tick = world.CurrentTick,
                    Type = (int)EventType.BuildingRemoved,
                    EntityId = building.Id,
                    OwnerId = building.OwnerId,
                    Position = building.Position
                });
            }
        }
    }

    private static void CleanupResources(RuntimeContext context)
    {
        foreach (var resource in WorldQueries.FindDepletedResources(context.World))
        {
            context.World.Entities.Remove(resource);
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

        var player = world.GetPlayerById(building.OwnerId);
        if (player == null)
        {
            return;
        }

        PopulationActions.DecreaseCap(
            player,
            building.Definition.PopulationBonus);
    }
}
