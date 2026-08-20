using RTSEngine.Core.Commands;
using RTSEngine.Core.Entities.Buildings;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Entities.States;
using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.State;
using RTSEngine.Core.Systems;
using RTSEngine.Core.Actions;
using RTSEngine.Core.Simulation;
using RTSEngine.Core.Players;
using RTSEngine.Core.Entities.Definitions;
using RTSEngine.Core.Helpers;
using RTSEngine.Tests.TestHelpers;

namespace RTSEngine.Tests.BuildingTests;

public class BuildingDestructionTests
{
    [Fact]
    [Trait("Category", "Building")]
    [Trait("Category", "Destruction")]
    public void Building_ShouldDie_WhenHealthReachesZero()
    {
        var building = BuildingFactory.Create(
            TestDefinitionFactory.CreateTownCenter(),
            ownerId: 1,
            position: new GridPosition(5, 5));
        building.IsCompleted = true;
        building.CurrentHealth = 10;

        Assert.False(building.IsDead);

        building.TakeDamage(10);

        Assert.True(building.IsDead);
        Assert.Equal(0, building.CurrentHealth);
    }

    [Fact]
    [Trait("Category", "Building")]
    [Trait("Category", "Destruction")]
    public void DeadBuilding_ShouldNotBlockTiles()
    {
        var world = TestWorldFactory.CreateWorld();

        var building = BuildingFactory.Create(
            TestDefinitionFactory.CreateTownCenter(),
            ownerId: 1,
            position: new GridPosition(5, 5));
        building.IsCompleted = true;
        building.CurrentHealth = 1000;

        world.AddEntity(building);

        Assert.True(WorldQueries.IsTileBlocked(world, 5, 5));

        building.TakeDamage(building.CurrentHealth);

        Assert.False(WorldQueries.IsTileBlocked(world, 5, 5));
    }

    [Fact]
    [Trait("Category", "Building")]
    [Trait("Category", "Destruction")]
    public void RemoveDeadEntities_ShouldRemoveDeadBuildings()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();

        var building = BuildingFactory.Create(
            TestDefinitionFactory.CreateTownCenter(),
            ownerId: 1,
            position: new GridPosition(5, 5));
        building.IsCompleted = true;
        building.CurrentHealth = 1;

        world.AddEntity(building);

        var simulation = new SimulationRunner(
            new RuntimeContext
            {
                World = world,
                UnitRepository = new([]),
                BuildingRepository = new([])
            });

        building.TakeDamage(1);

        simulation.Step();

        Assert.DoesNotContain(building, world.Entities.ToList());
    }

    [Fact]
    [Trait("Category", "Building")]
    [Trait("Category", "Destruction")]
    public void RemoveDeadBuildings_ShouldReleaseBuilders()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();

        var building = BuildingFactory.Create(
            TestDefinitionFactory.CreateTownCenter(),
            ownerId: 1,
            position: new GridPosition(5, 5));
        building.IsCompleted = true;
        building.CurrentHealth = 1;

        var builder = UnitFactory.Create(
            TestDefinitionFactory.CreateVillager(),
            ownerId: 1,
            position: new GridPosition(7, 7));
        builder.CurrentTask = UnitTask.Building;
        builder.Build.BuildingId = building.Id;
        builder.Build.Phase = BuildPhase.Constructing;

        world.AddEntity(building);
        world.AddEntity(builder);

        var simulation = new SimulationRunner(
            new RuntimeContext
            {
                World = world,
                UnitRepository = new([]),
                BuildingRepository = new([])
            });

        building.TakeDamage(1);

        simulation.Step();

        Assert.Equal(UnitTask.Idle, builder.CurrentTask);
        Assert.Null(builder.Build.BuildingId);
    }

    [Fact]
    [Trait("Category", "Building")]
    [Trait("Category", "Destruction")]
    public void RemoveDeadBuildings_ShouldDecreasePopulationCap()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();

        var player = world.GetPlayerById(1)!;
        PopulationActions.IncreaseCap(player, 5);

        var building = BuildingFactory.Create(
            TestDefinitionFactory.CreateTownCenter(),
            ownerId: 1,
            position: new GridPosition(5, 5));
        building.IsCompleted = true;
        building.CurrentHealth = 1;

        world.AddEntity(building);

        int capBefore = player.Population.Capacity;

        var simulation = new SimulationRunner(
            new RuntimeContext
            {
                World = world,
                UnitRepository = new([]),
                BuildingRepository = new([])
            });

        building.TakeDamage(1);

        simulation.Step();

        Assert.True(player.Population.Capacity < capBefore);
    }

    [Fact]
    [Trait("Category", "Combat")]
    public void Militia_ShouldStopAttacking_WhenBuildingDies()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();

        var militiaDef = new UnitDefinition
        {
            Id = "militia",
            Name = "Militia",
            MaxHealth = 60,
            MovementSpeed = 1f,
            AttackDamage = 30,
            AttackRange = 1,
            AttackCooldownTicks = 1
        };

        var militia = UnitFactory.Create(
            militiaDef, 1, new GridPosition(2, 2));

        var building = BuildingFactory.Create(
            TestDefinitionFactory.CreateTownCenter(),
            ownerId: 2,
            position: new GridPosition(3, 2));
        building.IsCompleted = true;
        building.CurrentHealth = 30;

        world.AddEntity(militia);
        world.AddEntity(building);

        CombatSystem.BeginAttack(world, militia, building.Id);

        for (int i = 0; i < 20; i++)
        {
            MovementSystem.Update(world);
            CombatSystem.Update(world);
        }

        Assert.True(building.IsDead);
        Assert.Equal(UnitTask.Idle, militia.CurrentTask);
    }

    [Fact]
    [Trait("Category", "Building")]
    [Trait("Category", "Destruction")]
    public void Building_ShouldNotDie_BeforeCompletion()
    {
        var building = BuildingFactory.Create(
            TestDefinitionFactory.CreateTownCenter(),
            ownerId: 1,
            position: new GridPosition(5, 5));
        building.IsCompleted = false;
        building.CurrentHealth = 0;

        Assert.False(building.IsDead);
        Assert.True(building.IsBlocking);
    }

    [Fact]
    [Trait("Category", "Building")]
    [Trait("Category", "Destruction")]
    public void Building_ShouldNotDie_WhenHealthPositive()
    {
        var building = BuildingFactory.Create(
            TestDefinitionFactory.CreateTownCenter(),
            ownerId: 1,
            position: new GridPosition(5, 5));
        building.IsCompleted = true;
        building.CurrentHealth = 1;

        Assert.False(building.IsDead);
    }
}

public class BuildingRefundTests
{
    [Fact]
    [Trait("Category", "Building")]
    [Trait("Category", "Refund")]
    public void Refund_ShouldReturnResources()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var player = world.GetPlayerById(1)!;

        var building = BuildingFactory.Create(
            TestDefinitionFactory.CreateHouseWithCost(),
            ownerId: 1,
            position: new GridPosition(5, 5));

        int woodBefore = player.Economy.Get(ResourceType.Wood);

        EconomyActions.Refund(
            player,
            building.Definition.Costs);

        int woodAfter = player.Economy.Get(ResourceType.Wood);

        Assert.True(woodAfter > woodBefore);
    }

    [Fact]
    [Trait("Category", "Building")]
    [Trait("Category", "Refund")]
    public void DestroyedBuilding_ShouldNotAppear_InBuildingQueries()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();

        var building = BuildingFactory.Create(
            TestDefinitionFactory.CreateTownCenter(),
            ownerId: 1,
            position: new GridPosition(5, 5));
        building.IsCompleted = true;
        building.CurrentHealth = 1;

        world.AddEntity(building);

        var player = world.GetPlayerById(1)!;

        var simulation = new SimulationRunner(
            new RuntimeContext
            {
                World = world,
                UnitRepository = new([]),
                BuildingRepository = new([])
            });

        building.TakeDamage(1);

        simulation.Step();

        var buildings = world.Entities
            .OfType<Building>()
            .Where(b => b.OwnerId == 1)
            .ToList();

        Assert.DoesNotContain(building, buildings);
    }
}

public class UnitDeathPopulationTests
{
    [Fact]
    [Trait("Category", "Population")]
    [Trait("Category", "Destruction")]
    public void UnitDeath_ShouldDecrementPopulation()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var player = world.GetPlayerById(1)!;

        var unit = UnitFactory.Create(
            TestDefinitionFactory.CreateVillager(),
            ownerId: 1,
            position: new GridPosition(5, 5));
        world.AddEntity(unit);

        PopulationActions.AddPopulation(player, 1);
        int popBefore = player.Population.Current;

        unit.TakeDamage(unit.Definition.MaxHealth);

        var simulation = new SimulationRunner(
            new RuntimeContext
            {
                World = world,
                UnitRepository = new([]),
                BuildingRepository = new([])
            });

        simulation.Step();

        Assert.Equal(popBefore - 1, player.Population.Current);
        Assert.DoesNotContain(unit, world.Entities.ToList());
    }
}
