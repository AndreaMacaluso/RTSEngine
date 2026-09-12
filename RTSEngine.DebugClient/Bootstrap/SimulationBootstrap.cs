using RTSEngine.Core.Commands;
using RTSEngine.Core.Diagnostics;
using RTSEngine.Core.Map.Loading;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.State;
using RTSEngine.Core.Entities.Loaders;
using RTSEngine.Core.Entities.Definitions;
using RTSEngine.Core.Players;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Systems.Pathfinding;
using RTSEngine.Core.Settings;
using RTSEngine.Core.Triggers;

namespace RTSEngine.DebugClient.Bootstrap;

public static class SimulationBootstrap
{
    public static RuntimeContext Create(GameSettings? settings = null)
    {
        settings ??= new GameSettings();
        var baseDirectory = AppContext.BaseDirectory;

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

        var world = string.IsNullOrEmpty(settings.MapPath)
            ? CreateEmptyMap(settings.Width, settings.Height)
            : LoadWorld(Path.Combine(baseDirectory, settings.MapPath));

        foreach (var spawn in world.Spawns)
        {
            var player = new Player(spawn.PlayerId, "", ConsoleColor.Gray, PlayerControllerType.AI);
            world.AddPlayer(player);
        }
        
        var unitRepository = LoadUnitRepository(unitsPath);
        var buildingRepository = LoadBuildingRepository(buildingsPath);

        var triggerHandler = LoadTriggers(baseDirectory);

        return new RuntimeContext
        {
            World = world,
            UnitRepository = unitRepository,
            BuildingRepository = buildingRepository,
            CommandQueue = new CommandQueue(),
            PathFinder = new AStarPathFinder(new GroundMovementFilter()),
            Settings = settings,
            TriggerHandler = triggerHandler
        };
    }

    private static GameWorld CreateEmptyMap(int width, int height)
    {
        var map = new TileMap(width, height);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                map.SetTile(x, y, new Tile { TerrainType = TileType.Grass });
            }
        }

        return new GameWorld(map);
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

    private static TriggerHandler LoadTriggers(string baseDirectory)
    {
        var triggerHandler = new TriggerHandler();

        var triggerFiles = new[]
        {
            Path.Combine(baseDirectory, "Data", "Triggers", "CoreTrigger", "wave_system.json")
        };

        foreach (var triggerPath in triggerFiles)
        {
            DebugSession.Log.Debug($"[TriggerLoader] Looking for: {triggerPath}");
            DebugSession.Log.Debug($"[TriggerLoader] File exists: {File.Exists(triggerPath)}");

            if (File.Exists(triggerPath))
            {
                var mission = MissionLoader.Load(triggerPath);
                DebugSession.Log.Debug($"[TriggerLoader] Mission deserialized: {mission != null}");
                DebugSession.Log.Debug($"[TriggerLoader] Triggers count: {mission.Triggers.Count}");

                foreach (var triggerDef in mission.Triggers)
                {
                    DebugSession.Log.Debug($"[TriggerLoader] Creating trigger: {triggerDef.TriggerName}");
                    var trigger = TriggerFactory.Create(triggerDef);
                    triggerHandler.RegisterTrigger(trigger);
                    DebugSession.Log.Debug($"[TriggerLoader] Trigger registered: {trigger.Id}");
                }
            }
        }

        return triggerHandler;
    }
}