using RTSEngine.Core.Entities.Buildings;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Actions;
namespace RTSEngine.DebugClient.StartingConditions;

public static class EntitySpawner
{
    public static Unit SpawnVillager(
        RuntimeContext context,
        int ownerId,
        GridPosition position)
    {
        var definition =
            context.UnitRepository.Get("villager");

        var unit = UnitFactory.Create(
            definition,
            ownerId,
            position);

        context.World.AddEntity(unit);

        var player = context.World.GetPlayerById(ownerId)
            ?? throw new InvalidOperationException(
                $"Cannot spawn a villager for unknown player {ownerId}.");

        PopulationActions.AddPopulation(player, 1);

        return unit;
    }

    public static Building SpawnTownCenter(
        RuntimeContext context,
        int ownerId,
        GridPosition position)
    {
        var definition =
            context.BuildingRepository.Get("town_center");

        var building = BuildingFactory.Create(
            definition,
            ownerId,
            position);

        building.IsCompleted = true;
        building.CurrentHealth = definition.MaxHealth;

        context.World.AddEntity(building);

        var player = context.World.GetPlayerById(ownerId)
            ?? throw new InvalidOperationException(
                $"Cannot spawn a town center for unknown player {ownerId}.");

        PopulationActions.IncreaseCap(player, definition.PopulationBonus);

        return building;
    }
}
