using RTSEngine.Core.Actions;
using RTSEngine.Core.Commands;
using RTSEngine.Core.Entities;
using RTSEngine.Core.Entities.Buildings;
using RTSEngine.Core.Entities.Definitions;
using RTSEngine.Core.Entities.Resources;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Entities.States;
using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Events;
using RTSEngine.Core.Helpers;
using RTSEngine.Core.IO;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Map.Visibility;
using RTSEngine.Core.Players;
using RTSEngine.Core.Settings;
using RTSEngine.Core.Simulation;
using RTSEngine.Core.State;
using RTSEngine.Core.Systems.Pathfinding;
using RTSEngine.Tests.TestHelpers;

namespace RTSEngine.Tests.SaveLoad;

[Trait("Category", "SaveLoad")]
public class SaveLoadTests
{
    private static RuntimeContext CreateContext(GameWorld world)
    {
        var unitRepo = new UnitDefinitionRepository(new[]
        {
            TestDefinitionFactory.CreateVillager(),
            TestDefinitionFactory.CreateMilitiaWithCombatStats()
        });
        var buildingRepo = new BuildingDefinitionRepository(new[]
        {
            TestDefinitionFactory.CreateTownCenter(),
            TestDefinitionFactory.CreateHouse()
        });

        return new RuntimeContext
        {
            World = world,
            UnitRepository = unitRepo,
            BuildingRepository = buildingRepo,
            CommandQueue = new CommandQueue(),
            PathFinder = new AStarPathFinder(new GroundMovementFilter()),
            Settings = new GameSettings
            {
                Visibility = MapVisibility.Normal
            },
            Events = new EventBus()
        };
    }

    private static (GameWorld World, RuntimeContext Context) BuildRichWorld()
    {
        // Real fog: the round trip has to carry the mode and the tiles,
        // otherwise a save silently changes the rules.
        var world = TestWorldFactory.CreateWorldWithTwoPlayers(
            width: 40, height: 40, visibility: MapVisibility.Normal);
        var context = CreateContext(world);

        for (int x = 0; x < 40; x++)
            world.Map.SetTile(x, 0, new Tile { TerrainType = TileType.Water });
        for (int x = 0; x < 40; x++)
            world.Map.SetTile(x, 1, new Tile { TerrainType = TileType.Mountain });

        var p1 = world.GetPlayerById(1)!;
        var p2 = world.GetPlayerById(2)!;
        p1.Economy.Add(ResourceType.Food, 200);
        p1.Economy.Add(ResourceType.Wood, 200);
        p1.Population.Capacity = 200;
        p2.Population.Capacity = 200;

        var tc = BuildingFactory.Create(TestDefinitionFactory.CreateTownCenter(), 1, new GridPosition(5, 5));
        tc.IsCompleted = true;
        tc.Health.CurrentHealth = tc.Definition.MaxHealth;
        world.Entities.Add(tc, p1);

        var house = BuildingFactory.Create(TestDefinitionFactory.CreateHouse(), 1, new GridPosition(9, 9));
        house.ConstructionProgress = 5;
        house.Health.CurrentHealth = 50;
        world.Entities.Add(house, p1);

        var m1 = UnitFactory.Create(TestDefinitionFactory.CreateMilitiaWithCombatStats(), 1, new GridPosition(10, 10));
        var m2 = UnitFactory.Create(TestDefinitionFactory.CreateMilitiaWithCombatStats(), 1, new GridPosition(11, 10));
        var villager = UnitFactory.Create(TestDefinitionFactory.CreateVillager(), 1, new GridPosition(9, 11));
        var enemy = UnitFactory.Create(TestDefinitionFactory.CreateMilitiaWithCombatStats(), 2, new GridPosition(12, 10));

        world.Entities.Add(m1, p1);
        world.Entities.Add(m2, p1);
        world.Entities.Add(villager, p1);
        world.Entities.Add(enemy, p2);

        var tree = new Tree(new GridPosition(9, 12));
        world.Entities.Add(tree);
        var gold = new GoldMine(new GridPosition(20, 20));
        world.Entities.Add(gold);

        context.CommandQueue.Enqueue(new MoveCommand
        {
            UnitIds = [m1.Id],
            PlayerId = 1,
            Target = new GridPosition(15, 15)
        });
        context.CommandQueue.Enqueue(new AttackCommand
        {
            UnitIds = [m2.Id],
            PlayerId = 1,
            Mode = AttackMode.Entity,
            TargetEntityId = enemy.Id
        });
        context.CommandQueue.Enqueue(new GatherCommand
        {
            UnitIds = [villager.Id],
            PlayerId = 1,
            ResourceId = tree.Id
        });
        ProductionActions.TryTrainUnit(context, tc, "villager");

        return (world, context);
    }

    private static void RunTicks(RuntimeContext context, int ticks)
    {
        var simulation = new SimulationRunner(context);
        for (int i = 0; i < ticks; i++)
            simulation.Step();
    }

    [Fact]
    public void Roundtrip_ShouldPreserveFullState()
    {
        var (world, context) = BuildRichWorld();
        RunTicks(context, 120);

        var projectile = new Projectile
        {
            Id = world.Projectiles.NextId(),
            OwnerId = 1,
            X = new FixedPoint(1000),
            Y = new FixedPoint(2000),
            TargetX = new FixedPoint(1500),
            TargetY = new FixedPoint(2500),
            TargetEntityId = 1,
            Damage = 6,
            Speed = new FixedPoint(2000),
            IsSingleTarget = true,
            SplashRadius = FixedPoint.Zero,
            IsActive = true
        };
        world.Projectiles.Add(projectile);

        var original = GameSnapshot.Capture(world);
        var restoredWorld = GameSnapshot.Restore(original, context.UnitRepository, context.BuildingRepository);

        var recaptured = GameSnapshot.Capture(restoredWorld);

        Assert.Equal(original.Serialize(), recaptured.Serialize());
    }

    [Fact]
    public void SaveManager_ShouldRoundtripThroughFile()
    {
        var (world, context) = BuildRichWorld();
        RunTicks(context, 80);

        var path = Path.Combine(Path.GetTempPath(), $"rts_save_{Guid.NewGuid():N}.json");
        try
        {
            SaveManager.Save(world, context.Settings, path);

            var loaded = SaveManager.Load(path);
            var restoredWorld = GameSnapshot.Restore(loaded, context.UnitRepository, context.BuildingRepository);

            Assert.Equal(GameSnapshot.Capture(world).Serialize(), GameSnapshot.Capture(restoredWorld).Serialize());
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public void RestoredWorld_ShouldContinueDeterministically()
    {
        var (worldA, ctxA) = BuildRichWorld();
        var (worldB, ctxB) = BuildRichWorld();

        RunTicks(ctxA, 100);
        RunTicks(ctxB, 100);

        var snapshot = GameSnapshot.Capture(worldA);
        var restored = GameSnapshot.Restore(snapshot, ctxA.UnitRepository, ctxA.BuildingRepository);
        var restoredContext = CreateContext(restored);

        RunTicks(ctxA, 50);
        RunTicks(restoredContext, 50);

        Assert.Equal(
            GameSnapshot.Capture(worldA).Serialize(),
            GameSnapshot.Capture(restored).Serialize());
    }

    [Fact]
    public void Settings_ShouldSurviveRoundtrip()
    {
        var (world, context) = BuildRichWorld();
        RunTicks(context, 5);

        var snapshot = GameSnapshot.Capture(world, context.Settings);

        Assert.Equal(nameof(MapVisibility.Normal), snapshot.Settings.Visibility);
        Assert.Equal(context.Settings.PopulationCap, snapshot.Settings.PopulationCap);
        Assert.Equal(context.Settings.NumPlayers, snapshot.Settings.NumPlayers);
        Assert.Equal(context.Settings.Speed.ToString(), snapshot.Settings.Speed);

        var restored = GameSnapshot.Restore(snapshot, context.UnitRepository, context.BuildingRepository);
        var recaptured = GameSnapshot.Capture(restored);

        Assert.True(restored.Fog.IsEnabled);
        Assert.Equal(snapshot.Settings.Visibility, recaptured.Settings.Visibility);
    }

    [Fact]
    public void Fog_ShouldSurviveRoundtrip_EvenWithoutSettings()
    {
        var (world, context) = BuildRichWorld();
        RunTicks(context, 5);

        // No settings passed: the mode still has to come from the world,
        // not from the default, or a load would turn the fog off.
        var snapshot = GameSnapshot.Capture(world);

        Assert.Equal(nameof(MapVisibility.Normal), snapshot.Settings.Visibility);
        Assert.NotEmpty(snapshot.Fog);
        Assert.Contains(snapshot.Fog, f => f.Tiles.Contains((int)TileVisibility.Visible));

        var restored = GameSnapshot.Restore(snapshot, context.UnitRepository, context.BuildingRepository);
        var recaptured = GameSnapshot.Capture(restored);

        Assert.True(restored.Fog.IsEnabled);
        Assert.Equal(snapshot.Serialize(), recaptured.Serialize());
    }

    [Fact]
    public void Load_ShouldRejectUnsupportedFormatVersion()
    {
        var (world, context) = BuildRichWorld();
        RunTicks(context, 5);

        var path = Path.Combine(Path.GetTempPath(), $"rts_save_{Guid.NewGuid():N}.json");
        try
        {
            SaveManager.Save(world, context.Settings, path);

            var json = File.ReadAllText(path)
                .Replace($"\"format_version\":{GameSnapshot.CurrentFormatVersion}",
                         "\"format_version\":999");
            File.WriteAllText(path, json);

            var ex = Assert.Throws<InvalidDataException>(() => SaveManager.Load(path));
            Assert.Contains("999", ex.Message);
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public void CombatResume_ShouldSurviveRoundtrip()
    {
        var (world, context) = BuildRichWorld();

        var unit = world.Entities.Units.OrderBy(u => u.Id).First();
        unit.Combat.ResumePhase = CombatPhase.AttackMoving;
        unit.Combat.ResumeDestination = new GridPosition(8, 8);

        var snapshot = GameSnapshot.Capture(world);
        var restored = GameSnapshot.Restore(snapshot, context.UnitRepository, context.BuildingRepository);

        var restoredUnit = restored.Entities.Units.Single(u => u.Id == unit.Id);
        Assert.Equal(CombatPhase.AttackMoving, restoredUnit.Combat.ResumePhase);
        Assert.Equal(new GridPosition(8, 8), restoredUnit.Combat.ResumeDestination);

        var recaptured = GameSnapshot.Capture(restored);
        Assert.Equal(snapshot.Serialize(), recaptured.Serialize());
    }
}
