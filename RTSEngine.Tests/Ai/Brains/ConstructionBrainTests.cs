using RTSEngine.Core.AI;
using RTSEngine.Core.AI.Brains;
using RTSEngine.Core.Commands;
using RTSEngine.Core.Entities.Buildings;
using RTSEngine.Core.Entities.Definitions;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Players;
using RTSEngine.Core.State;
using RTSEngine.Core.Settings;
using RTSEngine.Core.Systems.Pathfinding;
using RTSEngine.Tests.TestHelpers;

namespace RTSEngine.Tests.AI.Brains;

public class ConstructionBrainTests
{
    private readonly RuntimeContext _context;
    private readonly GameWorld _world;
    private readonly Player _player;

    public ConstructionBrainTests()
    {
        _context = new RuntimeContext
        {
            World = TestWorldFactory.CreateWorldWithTwoPlayers(),
            UnitRepository = new UnitDefinitionRepository([]),
            BuildingRepository = new BuildingDefinitionRepository([
                TestDefinitionFactory.CreateTownCenter(),
                TestDefinitionFactory.CreateHouse(),
                TestDefinitionFactory.CreateBarracks()
            ]),
            CommandQueue = new CommandQueue(),
            PathFinder = new AStarPathFinder(new GroundMovementFilter()),
            Settings = new GameSettings()
        };

        _world = _context.World;
        _player = _world.GetPlayerById(1)!;
    }

    private Building CreateCompletedBuilding(BuildingDefinition def, int ownerId, GridPosition pos)
    {
        var building = BuildingFactory.Create(def, ownerId, pos);
        building.IsCompleted = true;
        return building;
    }

    [Fact(Skip = "AI Brain in a momentary code, waiting for lua integration")]
    [Trait("Category", "AI")]
    [Trait("Category", "Brain")]
    public void ShouldBuildHouse_WhenPopulationNearCap()
    {
        var tc = CreateCompletedBuilding(
            TestDefinitionFactory.CreateTownCenter(),
            _player.Id,
            new GridPosition(0, 0));
        _world.Entities.Add(tc, _player);

        _player.Population.Current = 3;
        _player.Population.Capacity = 5;

        var brain = new ConstructionBrain();
        brain.Execute(_context, _player);

        Assert.Contains(_context.CommandQueue.Pending, c => c is BuildCommand);
    }

    [Fact(Skip = "AI Brain in a momentary code, waiting for lua integration")]
    [Trait("Category", "AI")]
    [Trait("Category", "Brain")]
    public void ShouldBuildBarracks_WhenPopReachedTarget()
    {
        var tc = CreateCompletedBuilding(
            TestDefinitionFactory.CreateTownCenter(),
            _player.Id,
            new GridPosition(0, 0));
        _world.Entities.Add(tc, _player);

        _player.Population.Current = 15;
        _player.Population.Capacity = 20;

        var brain = new ConstructionBrain();
        brain.Execute(_context, _player);

        Assert.Contains(_context.CommandQueue.Pending, c => c is BuildCommand);
    }

    [Fact]
    [Trait("Category", "AI")]
    [Trait("Category", "Brain")]
    public void ShouldDoNothing_WhenPopOkAndBarracksExist()
    {
        var tc = CreateCompletedBuilding(
            TestDefinitionFactory.CreateTownCenter(),
            _player.Id,
            new GridPosition(0, 0));
        _world.Entities.Add(tc, _player);

        var barracks = CreateCompletedBuilding(
            TestDefinitionFactory.CreateBarracks(),
            _player.Id,
            new GridPosition(5, 0));
        _world.Entities.Add(barracks, _player);

        _player.Population.Current = 15;
        _player.Population.Capacity = 20;

        var brain = new ConstructionBrain();
        brain.Execute(_context, _player);

        Assert.Empty(_context.CommandQueue.Pending);
    }

    [Fact(Skip = "AI Brain in a momentary code, waiting for lua integration")]
    [Trait("Category", "AI")]
    [Trait("Category", "Brain")]
    public void ShouldBuildBarracks_WhenNoBarracksEvenWithoutTC()
    {
        _player.Population.Current = 15;
        _player.Population.Capacity = 20;

        var brain = new ConstructionBrain();
        brain.Execute(_context, _player);

        Assert.Contains(_context.CommandQueue.Pending, c => c is BuildCommand);
    }
}
