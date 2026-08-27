using RTSEngine.Core.Commands;
using RTSEngine.Core.Map.Loading;
using RTSEngine.Core.State;
using RTSEngine.Core.Entities.Loaders;
using RTSEngine.Core.Entities.Definitions;
using RTSEngine.Core.Players;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Systems.Pathfinding;

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

        var world = LoadWorld(mapPath);

        foreach (var spawn in world.Spawns)
        {
            var player = new Player(spawn.PlayerId, "", ConsoleColor.Gray, PlayerControllerType.AI);
            world.AddPlayer(player);
        }
        
        var unitRepository = LoadUnitRepository(unitsPath);
        var buildingRepository = LoadBuildingRepository(buildingsPath);

        return new RuntimeContext
        {
            World = world,
            UnitRepository = unitRepository,
            BuildingRepository = buildingRepository,
            CommandQueue = new CommandQueue(),
            PathFinder = new AStarPathFinder(new GroundMovementFilter())
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
        var unitDefinitions = DefinitionLoader<UnitDefinition>.Load(unitsPath);

        return new UnitDefinitionRepository(unitDefinitions);
    }

    private static BuildingDefinitionRepository LoadBuildingRepository(
    string buildingsPath)
    {
        var definitions = DefinitionLoader<BuildingDefinition>.Load(buildingsPath);

        return new BuildingDefinitionRepository(definitions);
    }
}