using RTSEngine.Core.Commands;
using RTSEngine.Core.Diagnostics;
using RTSEngine.Core.Map.Loading;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Map.Definitions;
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
            ? CreateEmptyMap(settings)
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

    private static GameWorld CreateEmptyMap(GameSettings settings)
    {
        var map = new TileMap(settings.Width, settings.Height);

        for (int x = 0; x < settings.Width; x++)
        {
            for (int y = 0; y < settings.Height; y++)
            {
                map.SetTile(x, y, new Tile { TerrainType = TileType.Grass });
            }
        }

        var spawns = GenerateSpawnPoints(settings);
        return new GameWorld(map, spawns: spawns);
    }

    private static List<SpawnPointDefinition> GenerateSpawnPoints(GameSettings settings)
    {
        var spawns = new List<SpawnPointDefinition>();
        int numPlayers = settings.NumPlayers;
        int width = settings.Width;
        int height = settings.Height;
        int margin = 5;

        for (int i = 0; i < numPlayers; i++)
        {
            double angle = 2 * Math.PI * i / numPlayers;
            int x = (int)(width / 2 + (width / 2 - margin) * Math.Cos(angle));
            int y = (int)(height / 2 + (height / 2 - margin) * Math.Sin(angle));

            x = Math.Clamp(x, margin, width - margin - 1);
            y = Math.Clamp(y, margin, height - margin - 1);

            spawns.Add(new SpawnPointDefinition
            {
                PlayerId = i + 1,
                X = x,
                Y = y
            });
        }

        return spawns;
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