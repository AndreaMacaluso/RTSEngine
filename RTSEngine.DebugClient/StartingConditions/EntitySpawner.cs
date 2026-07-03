using RTSEngine.Core.Entities.Buildings;
using RTSEngine.Core.Entities.Definitions;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.DebugClient.Bootstrap;

namespace RTSEngine.DebugClient.StartingConditions;

public static class EntitySpawner
{
    public static Unit SpawnVillager(
        SimulationContext context,
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

        return unit;
    }

    public static Building SpawnTownCenter(
        SimulationContext context,
        int ownerId,
        GridPosition position)
    {
        var definition =
            context.BuildingRepository.Get("town_center");

        var building = BuildingFactory.Create(
            definition,
            ownerId,
            position);

        context.World.AddEntity(building);

        return building;
    }
}