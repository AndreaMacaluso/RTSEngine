using RTSEngine.Core.Map.Loading;
using RTSEngine.Core.State;
using RTSEngine.Core.Entities.Loader;
using RTSEngine.Core.Entities.Definitions;
using RTSEngine.Core.Players;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Systems;

namespace RTSEngine.DebugClient.Bootstrap;

public static class SimulationBootstrap
{
    public static RuntimeContext Create()
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
            "Buildings",
            "buildings.json");

        var scriptsPath = Path.Combine(
            baseDirectory,
            "Data",
            "AI");

        var world = LoadWorld(mapPath);

        foreach (var spawn in world.Spawns)
        {
            var player = PlayerFactory.Create(spawn.PlayerId);
            player.Controller = PlayerControllerType.AI;
            world.AddPlayer(player);
        }
        
        var unitRepository = LoadUnitRepository(unitsPath);
        var buildingRepository = LoadBuildingRepository(buildingsPath);

        var context = new RuntimeContext
        {
            World = world,
            UnitRepository = unitRepository,
            BuildingRepository = buildingRepository
        };

        AISystem.Initialize(scriptsPath);

        foreach (var player in world.Players)
        {
            if (player.Controller == PlayerControllerType.AI)
            {
                AISystem.LoadLuaScript(context, player, "base_ai");
            }
        }

        return context;
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