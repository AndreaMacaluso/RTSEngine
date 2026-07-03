using RTSEngine.Core.Map.Loading;
using RTSEngine.Core.State;
using RTSEngine.Core.Entities.Loader;
using RTSEngine.Core.Entities.Definitions;
using RTSEngine.Core.Players;

namespace RTSEngine.DebugClient.Bootstrap;

public static class SimulationBootstrap
{
    public static SimulationContext Create()
    {
        var baseDirectory = AppContext.BaseDirectory;

        var mapPath = Path.Combine(
            baseDirectory,
            "Data",
            "Maps",
            "map_00.json");

        var unitsPath = Path.Combine(
            baseDirectory,
            "Data",
            "Units",
            "units.json");

        var buildingsPath = Path.Combine(
            baseDirectory,
            "Data",
            "Units",
            "buildings.json");

        var world = LoadWorld(mapPath);

        foreach (var spawn in world.Spawns)
        {
            var player = PlayerFactory.Create(spawn.PlayerId);
            world.AddPlayer(player);
        }
        
        var unitRepository = LoadUnitRepository(unitsPath);
        var buildingRepository = LoadBuildingRepository(buildingsPath);

        return new SimulationContext
        {
            World = world,
            UnitRepository = unitRepository,
            BuildingRepository = buildingRepository
        };
    }

    private static GameWorld LoadWorld(string mapPath)
    {
        var mapLoader = new JsonMapLoader();
        var mapData = mapLoader.Load(mapPath);

        return WorldBuilder.Build(mapData);
    }

    private static UnitDefinitionRepository LoadUnitRepository(
        string unitsPath)
    {
        var unitLoader = new UnitDefinitionLoader();
        var unitDefinitions = unitLoader.Load(unitsPath);

        return new UnitDefinitionRepository(unitDefinitions);
    }

    private static BuildingDefinitionRepository LoadBuildingRepository(
    string buildingsPath)
    {
        var loader = new BuildingDefinitionLoader();

        var definitions = loader.Load(buildingsPath);

        return new BuildingDefinitionRepository(definitions);
    }
}