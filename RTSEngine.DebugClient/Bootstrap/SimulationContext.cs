using RTSEngine.Core.Map.Loading;
using RTSEngine.Core.State;
using RTSEngine.Core.Entities.Loader;
using RTSEngine.Core.Entities.Definitions;

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

        var world = LoadWorld(mapPath);
        var unitRepository = LoadUnitRepository(unitsPath);

        return new SimulationContext
        {
            World = world,
            UnitRepository = unitRepository
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
}