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
using RTSEngine.Core.Settings;
using RTSEngine.Core.Systems.Pathfinding;
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
        building.Health.CurrentHealth = 10;

        Assert.False(building.IsDead);

        building.Health.TakeDamage(10);

        Assert.True(building.IsDead);
        Assert.Equal(0, building.Health.CurrentHealth);
    }

    [Fact]
    [Trait("Category", "Building")]
    [Trait("Category", "Destruction")]
    public void DeadBuilding_ShouldNotBlockTiles()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var player = world.GetPlayerById(1)!;

        var building = BuildingFactory.Create(
            TestDefinitionFactory.CreateTownCenter(),
            ownerId: 1,
            position: new GridPosition(5, 5));
        building.IsCompleted = true;
        building.Health.CurrentHealth = 1000;

        world.Entities.Add(building, player);

        Assert.True(WorldQueries.IsTileBlocked(world, 5, 5));

        building.Health.TakeDamage(building.Health.CurrentHealth);
        world.Entities.RebuildSpatialIndex();

        Assert.False(WorldQueries.IsTileBlocked(world, 5, 5));
    }

    [Fact]
    [Trait("Category", "Building")]
    [Trait("Category", "Destruction")]
    public void RemoveDeadEntities_ShouldRemoveDeadBuildings()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var player = world.GetPlayerById(1)!;

        var building = BuildingFactory.Create(
            TestDefinitionFactory.CreateTownCenter(),
            ownerId: 1,
            position: new GridPosition(5, 5));
        building.IsCompleted = true;
        building.Health.CurrentHealth = 1;

        world.Entities.Add(building, player);

        var simulation = new SimulationRunner(
            SimulationTestHelper.CreateContext(
                world,
                engine: new EngineSettings { DecayTicks = 0 }));

        building.Health.TakeDamage(1);

        simulation.Step();

        Assert.DoesNotContain(building, world.Entities.Buildings.Values.ToList());
    }

    [Fact]
    [Trait("Category", "Building")]
    [Trait("Category", "Destruction")]
    public void RemoveDeadBuildings_ShouldReleaseBuilders()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var player = world.GetPlayerById(1)!;

        var building = BuildingFactory.Create(
            TestDefinitionFactory.CreateTownCenter(),
            ownerId: 1,
            position: new GridPosition(5, 5));
        building.IsCompleted = true;
        building.Health.CurrentHealth = 1;

        var builder = UnitFactory.Create(
            TestDefinitionFactory.CreateVillager(),
            ownerId: 1,
            position: new GridPosition(7, 7));
        builder.CurrentTask = EntityState.Building;
        builder.Build.BuildingId = building.Id;
        builder.Build.Phase = BuildPhase.Constructing;

        world.Entities.Add(building, player);
        world.Entities.Add(builder, player);

        var simulation = new SimulationRunner(
            SimulationTestHelper.CreateContext(
                world,
                engine: new EngineSettings { DecayTicks = 0 }));

        building.Health.TakeDamage(1);

        simulation.Step();

        Assert.Equal(EntityState.Idle, builder.CurrentTask);
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
        building.Health.CurrentHealth = 1;

        world.Entities.Add(building, player);

        int capBefore = player.Population.Capacity;

        var simulation = new SimulationRunner(
            SimulationTestHelper.CreateContext(
                world,
                engine: new EngineSettings { DecayTicks = 0 }));

        building.Health.TakeDamage(1);

        simulation.Step();

        Assert.Empty(world.Entities.Buildings.Values);
    }


    [Fact]
    [Trait("Category", "Combat")]
    public void Militia_ShouldStopAttacking_WhenBuildingDies()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var context = SimulationTestHelper.CreateContext(world);
        var player1 = world.GetPlayerById(1)!;
        var player2 = world.GetPlayerById(2)!;

        var militiaDef = new UnitDefinition
        {
            Id = "militia",
            Name = "Militia",
            MaxHealth = 60,
            MovementSpeed = 1f,
            MeleeAttack = 30,
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
        building.Health.CurrentHealth = 30;

        world.Entities.Add(militia, player1);
        world.Entities.Add(building, player2);

        CombatSystem.BeginAttack(world, militia, building.Id);

        for (int i = 0; i < 20; i++)
        {
            MovementSystem.Update(context);
            CombatSystem.Update(context);
        }

        Assert.True(building.IsDead);
        Assert.Equal(EntityState.Idle, militia.CurrentTask);
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
        building.Health.CurrentHealth = 0;

        Assert.False(building.IsDead);
        Assert.False(building.IsBlocking);
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
        building.Health.CurrentHealth = 1;

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
        building.Health.CurrentHealth = 1;

        var player = world.GetPlayerById(1)!;
        world.Entities.Add(building, player);

        var simulation = new SimulationRunner(
            SimulationTestHelper.CreateContext(
                world,
                engine: new EngineSettings { DecayTicks = 0 }));

        building.Health.TakeDamage(1);

        simulation.Step();

        var buildings = world.Entities.Buildings.Values
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
        world.Entities.Add(unit, player);

        PopulationActions.AddPopulation(player, 1);
        int popBefore = player.Population.Current;

        unit.Health.TakeDamage(unit.Definition.MaxHealth);

        var simulation = new SimulationRunner(
            SimulationTestHelper.CreateContext(world));

        simulation.Step();

        Assert.Equal(popBefore - 1, player.Population.Current);
        Assert.Equal(EntityState.Decaying, unit.CurrentTask);
    }
}
