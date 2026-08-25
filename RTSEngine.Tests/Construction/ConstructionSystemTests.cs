using RTSEngine.Core.Actions;
using RTSEngine.Core.Entities.States;
using RTSEngine.Core.Entities.Buildings;
using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Systems;
using RTSEngine.Tests.TestHelpers;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.State;
using RTSEngine.Core.Commands;
using RTSEngine.Core.Systems.Pathfinding;

namespace RTSEngine.Tests.Construction;

public class ConstructionSystemTests
{
    private readonly RuntimeContext _context;
    private readonly GameWorld _world;
    private readonly Unit _villager;

    public ConstructionSystemTests()
    {
        _world = TestWorldFactory.CreateWorldWithTwoPlayers();

        var player = _world.GetPlayerById(1)!;

        _villager = UnitFactory.Create(
            TestDefinitionFactory.CreateVillager(),
            1,
            new GridPosition(1, 1));
        _world.Entities.Add(_villager, player);

        _context = new RuntimeContext
        {
            World = _world,
            UnitRepository = new RTSEngine.Core.Entities.Definitions.UnitDefinitionRepository([]),
            BuildingRepository = new RTSEngine.Core.Entities.Definitions.BuildingDefinitionRepository([]),
            CommandQueue = new CommandQueue(),
            PathFinder = new AStarPathFinder(new GroundMovementFilter())
        };
    }

    [Fact]
    [Trait("Category", "Building")]
    public void Update_ShouldSwitchToConstructing_WhenDestinationReached()
    {
        var player = _world.GetPlayerById(1)!;
        var building = BuildingFactory.Create(
            TestDefinitionFactory.CreateHouse(),
            1,
            new GridPosition(1, 2));
        _world.Entities.Add(building, player);

        _villager.Movement.NeedsRepath = true;
        _villager.Build.BuildPosition = building.Position;
        _villager.Build.BuildingId = building.Id;
        _villager.Build.Phase = BuildPhase.MovingToConstruction;
        _villager.Movement.PathQueue.Clear();

        SimulationTestHelper.RunTicks(_world, 10);

        Assert.Equal(BuildPhase.Constructing, _villager.Build.Phase);
    }

    [Fact]
    [Trait("Category", "Building")]
    public void Update_ShouldCompleteBuilding_WhenProgressFinishes()
    {
        var player = _world.GetPlayerById(1)!;
        var building = BuildingFactory.Create(
            TestDefinitionFactory.CreateHouse(),
            1,
            new GridPosition(1, 5));
        _world.Entities.Add(building, player);

        _villager.Build.BuildingId = building.Id;
        _villager.Build.Phase = BuildPhase.Constructing;
        Assert.True(_villager.Definition.CanBuild);
        building.ConstructionProgress = building.Definition.BuildTimeTicks - 1;

        SimulationTestHelper.RunTicks(_world, 10);

        Assert.True(building.IsCompleted);
        Assert.Equal(UnitTask.Idle, _villager.CurrentTask);
        Assert.Equal(BuildPhase.None, _villager.Build.Phase);
    }

    [Fact]
    [Trait("Category", "Building")]
    [Trait("Category", "BuildingLoop")]
    public void Builder_Should_ConstructBuilding_FromStartToFinish()
    {
        _villager.Health.CurrentHealth = 50;

        var player = _world.GetPlayerById(1)!;
        var building = BuildingFactory.Create(
            TestDefinitionFactory.CreateHouse(),
            1,
            new GridPosition(5, 5));
        _world.Entities.Add(building, player);

        _context.CommandQueue.Enqueue(new BuildCommand
        {
            UnitIds = [_villager.Id],
            BuildingId = building.Id
        });

        SimulationTestHelper.RunTicks(_world, 50, _context);

        Assert.Equal(UnitTask.Idle, _villager.CurrentTask);
        Assert.Equal(BuildPhase.None, _villager.Build.Phase);
        Assert.Null(_villager.Build.BuildingId);
        Assert.True(building.IsCompleted);
        Assert.Equal(building.Definition.MaxHealth, building.Health.CurrentHealth);
    }

    [Fact]
    [Trait("Category", "Building")]
    public void BuildOneTick_ShouldReturnFalse_WhenBuildingIdIsNull()
    {
        _villager.Build.BuildingId = null;
        _villager.Build.Phase = BuildPhase.Constructing;

        var player = _world.GetPlayerById(1)!;
        var building = BuildingFactory.Create(
            TestDefinitionFactory.CreateHouse(),
            1,
            new GridPosition(1, 2));
        _world.Entities.Add(building, player);

        var result = ConstructionActions.BuildOneTick(_world, _villager);

        Assert.False(result);
        Assert.Equal(0, building.ConstructionProgress);
    }

    [Fact]
    [Trait("Category", "Building")]
    public void CompleteConstruction_ShouldNotDoubleIncrement_WhenAlreadyCompleted()
    {
        var player = _world.GetPlayerById(1)!;
        var building = BuildingFactory.Create(
            TestDefinitionFactory.CreateTownCenter(),
            1,
            new GridPosition(1, 2));
        building.IsCompleted = true;
        building.Health.CurrentHealth = building.Health.MaxHealth;
        _world.Entities.Add(building, player);

        _villager.Build.BuildingId = building.Id;
        _villager.Build.Phase = BuildPhase.Constructing;

        int popBefore = player.Population.Capacity;

        ConstructionActions.CompleteConstruction(_world, _villager);

        Assert.Equal(popBefore, player.Population.Capacity);
    }

   
    [Fact]
    [Trait("Category", "Building")]
    public void HandleConstructing_ShouldStopBuilding_WhenBuildingAlreadyCompleted()
    {
        var player = _world.GetPlayerById(1)!;
        var building = BuildingFactory.Create(
            TestDefinitionFactory.CreateHouse(),
            1,
            new GridPosition(1, 2));
        building.IsCompleted = true;
        building.Health.CurrentHealth = building.Health.MaxHealth;
        _world.Entities.Add(building, player);

        _villager.Build.BuildingId = building.Id;
        _villager.Build.Phase = BuildPhase.Constructing;

        ConstructionSystem.Update(_context);

        Assert.Equal(BuildPhase.None, _villager.Build.Phase);
        Assert.Null(_villager.Build.BuildingId);
        Assert.Equal(UnitTask.Idle, _villager.CurrentTask);
    }
}
