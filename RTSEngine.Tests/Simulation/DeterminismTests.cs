using RTSEngine.Core.Commands;
using RTSEngine.Core.Entities;
using RTSEngine.Core.Entities.Buildings;
using RTSEngine.Core.Entities.Definitions;
using RTSEngine.Core.Entities.Resources;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Events;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Map.Visibility;
using RTSEngine.Core.Players;
using RTSEngine.Core.Settings;
using RTSEngine.Core.Simulation;
using RTSEngine.Core.State;
using RTSEngine.Core.Systems.Pathfinding;
using RTSEngine.Core.Triggers;
using RTSEngine.Core.Actions;
using RTSEngine.Tests.TestHelpers;

namespace RTSEngine.Tests.Simulation;

[Trait("Category", "Determinism")]
public class DeterminismTests
{
    private static RuntimeContext CreateContext(GameWorld world, bool fog)
    {
        var unitRepo = new UnitDefinitionRepository(new[]
        {
            TestDefinitionFactory.CreateVillager(),
            TestDefinitionFactory.CreateMilitiaWithCombatStats()
        });
        var buildingRepo = new BuildingDefinitionRepository(new[]
        {
            TestDefinitionFactory.CreateTownCenter()
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
                Visibility = fog ? VisibilityMode.FogOfWar : VisibilityMode.FullMap
            },
            Events = new EventBus()
        };
    }

    private static (GameWorld World, RuntimeContext Context) BuildScenario(bool fog = false)
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers(width: 40, height: 40);
        var context = CreateContext(world, fog);

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

        context.CommandQueue.Enqueue(new MoveCommand
        {
            UnitIds = [m1.Id],
            Target = new GridPosition(15, 15)
        });
        context.CommandQueue.Enqueue(new AttackCommand
        {
            UnitIds = [m2.Id],
            Mode = AttackMode.Entity,
            TargetEntityId = enemy.Id
        });
        context.CommandQueue.Enqueue(new GatherCommand
        {
            UnitIds = [villager.Id],
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

    private static void AssertWorldEqual(GameWorld a, GameWorld b)
    {
        Assert.Equal(a.CurrentTick, b.CurrentTick);
        Assert.Equal(a.State, b.State);

        Assert.Equal(a.Entities.Units.Count, b.Entities.Units.Count);
        Assert.Equal(a.Entities.Buildings.Count, b.Entities.Buildings.Count);
        Assert.Equal(a.Entities.Resources.Count, b.Entities.Resources.Count);

        for (int i = 0; i < a.Entities.Units.Count; i++)
        {
            var ua = a.Entities.Units[i];
            var ub = b.Entities.Units[i];
            Assert.Equal(ua.Id, ub.Id);
            Assert.Equal(ua.OwnerId, ub.OwnerId);
            Assert.Equal(ua.Position, ub.Position);
            Assert.Equal(ua.Health.CurrentHealth, ub.Health.CurrentHealth);
            Assert.Equal(ua.CurrentTask, ub.CurrentTask);
            Assert.Equal(ua.Stance, ub.Stance);
            Assert.Equal(ua.Combat.Phase, ub.Combat.Phase);
            Assert.Equal(ua.Combat.TargetEntityId, ub.Combat.TargetEntityId);
            Assert.Equal(ua.Gather.Phase, ub.Gather.Phase);
            Assert.Equal(ua.Gather.CarriedResource, ub.Gather.CarriedResource);
        }

        for (int i = 0; i < a.Entities.Buildings.Count; i++)
        {
            var ba = a.Entities.Buildings[i];
            var bb = b.Entities.Buildings[i];
            Assert.Equal(ba.Id, bb.Id);
            Assert.Equal(ba.OwnerId, bb.OwnerId);
            Assert.Equal(ba.Position, bb.Position);
            Assert.Equal(ba.Health.CurrentHealth, bb.Health.CurrentHealth);
            Assert.Equal(ba.IsCompleted, bb.IsCompleted);
            Assert.Equal(ba.ConstructionProgress, bb.ConstructionProgress);
        }

        for (int i = 0; i < a.Entities.Resources.Count; i++)
        {
            var ra = a.Entities.Resources[i];
            var rb = b.Entities.Resources[i];
            Assert.Equal(ra.Id, rb.Id);
            Assert.Equal(ra.Position, rb.Position);
            Assert.Equal(ra.ResourceType, rb.ResourceType);
            Assert.Equal(ra.Amount, rb.Amount);
        }

        for (int i = 0; i < a.Players.Count; i++)
        {
            var pa = a.Players[i];
            var pb = b.Players[i];
            Assert.Equal(pa.Score, pb.Score);
            foreach (var type in new[] { ResourceType.Food, ResourceType.Wood, ResourceType.Gold, ResourceType.Stone })
            {
                Assert.Equal(pa.Economy.Get(type), pb.Economy.Get(type));
            }
            Assert.Equal(pa.Population.Current, pb.Population.Current);
            Assert.Equal(pa.Population.Capacity, pb.Population.Capacity);
            Assert.Equal(pa.Population.Reserved, pb.Population.Reserved);
        }
    }

    private static void AssertFogEqual(GameWorld a, GameWorld b)
    {
        Assert.Equal(a.Fog.Count, b.Fog.Count);
        foreach (var kvp in a.Fog)
        {
            var fb = b.Fog[kvp.Key];
            var fa = kvp.Value;
            for (int x = 0; x < fa.Width; x++)
            {
                for (int y = 0; y < fa.Height; y++)
                {
                    Assert.Equal(fa.Get(x, y), fb.Get(x, y));
                }
            }
        }
    }

    [Fact]
    public void SameInput_ShouldProduceIdenticalSnapshots_AfterTicks()
    {
        var (worldA, ctxA) = BuildScenario();
        var (worldB, ctxB) = BuildScenario();

        RunTicks(ctxA, 150);
        RunTicks(ctxB, 150);

        var snapA = GameSnapshot.Capture(worldA).Serialize();
        var snapB = GameSnapshot.Capture(worldB).Serialize();

        Assert.Equal(snapA, snapB);
    }

    [Fact]
    public void SameInput_ShouldProduceIdenticalWorldState_AfterTicks()
    {
        var (worldA, ctxA) = BuildScenario();
        var (worldB, ctxB) = BuildScenario();

        RunTicks(ctxA, 150);
        RunTicks(ctxB, 150);

        AssertWorldEqual(worldA, worldB);
    }

    [Fact]
    public void IdenticalSeed_ShouldProduceIdenticalMap()
    {
        var mapA = TestWorldFactory.CreateWorldWithTwoPlayers(width: 40, height: 40);
        var mapB = TestWorldFactory.CreateWorldWithTwoPlayers(width: 40, height: 40);

        Assert.Equal(mapA.Map.Width, mapB.Map.Width);
        Assert.Equal(mapA.Map.Height, mapB.Map.Height);
    }

    [Fact]
    public void Combat_ShouldBeDeterministic_AcrossRuns()
    {
        var (worldA, ctxA) = BuildScenario();
        var (worldB, ctxB) = BuildScenario();

        RunTicks(ctxA, 120);
        RunTicks(ctxB, 120);

        AssertWorldEqual(worldA, worldB);

        var enemyA = worldA.Entities.Units.FirstOrDefault(u => u.OwnerId == 2);
        var enemyB = worldB.Entities.Units.FirstOrDefault(u => u.OwnerId == 2);
        Assert.Equal(enemyA?.Health.CurrentHealth, enemyB?.Health.CurrentHealth);
    }

    [Fact]
    public void Visibility_ShouldBeDeterministic_AcrossRuns()
    {
        var (worldA, ctxA) = BuildScenario(fog: true);
        var (worldB, ctxB) = BuildScenario(fog: true);

        RunTicks(ctxA, 50);
        RunTicks(ctxB, 50);

        AssertFogEqual(worldA, worldB);
    }

    [Fact]
    public void Trigger_ShouldBeDeterministic_AcrossRuns()
    {
        var (worldA, ctxA) = BuildScenario();
        var (worldB, ctxB) = BuildScenario();

        var trigger = new Trigger
        {
            Id = "looping_spawn",
            Enabled = true,
            Looping = true,
            Conditions =
            [
                new OwnObjectsCondition(1, "villager", 2)
            ],
            Effects =
            [
                new CreateObjectEffect(1, "villager", 30, 30)
            ]
        };

        ctxA.TriggerHandler.RegisterTrigger(trigger);
        ctxB.TriggerHandler.RegisterTrigger(trigger);

        RunTicks(ctxA, 30);
        RunTicks(ctxB, 30);

        AssertWorldEqual(worldA, worldB);
    }
}