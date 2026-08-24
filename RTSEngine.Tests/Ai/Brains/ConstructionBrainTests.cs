using RTSEngine.Core.AI;
using RTSEngine.Core.AI.Brains;
using RTSEngine.Core.Commands;
using RTSEngine.Core.Entities.Buildings;
using RTSEngine.Core.Entities.Definitions;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Players;
using RTSEngine.Core.State;
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
            CommandQueue = new CommandQueue()
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

    [Fact]
    [Trait("Category", "AI")]
    [Trait("Category", "Brain")]
    public void ShouldBuildHouse_WhenPopulationNearCap()
    {
        var tc = CreateCompletedBuilding(
            TestDefinitionFactory.CreateTownCenter(),
            _player.Id,
            new GridPosition(0, 0));
        _world.AddEntity(tc);

        _player.Population.Current = 3;
        _player.Population.Capacity = 5;

        var brain = new ConstructionBrain();
        brain.Execute(_context, _player);
    }

    [Fact]
    [Trait("Category", "AI")]
    [Trait("Category", "Brain")]
    public void ShouldBuildBarracks_WhenPopReachedTarget()
    {
        var tc = CreateCompletedBuilding(
            TestDefinitionFactory.CreateTownCenter(),
            _player.Id,
            new GridPosition(0, 0));
        _world.AddEntity(tc);

        _player.Population.Current = 15;
        _player.Population.Capacity = 20;

        var brain = new ConstructionBrain();
        brain.Execute(_context, _player);
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
        _world.AddEntity(tc);

        var barracks = CreateCompletedBuilding(
            TestDefinitionFactory.CreateBarracks(),
            _player.Id,
            new GridPosition(5, 0));
        _world.AddEntity(barracks);

        _player.Population.Current = 15;
        _player.Population.Capacity = 20;

        var brain = new ConstructionBrain();
        brain.Execute(_context, _player);
    }

    [Fact]
    [Trait("Category", "AI")]
    [Trait("Category", "Brain")]
    public void ShouldBuildBarracks_WhenNoBarracksEvenWithoutTC()
    {
        _player.Population.Current = 15;
        _player.Population.Capacity = 20;

        var brain = new ConstructionBrain();
        brain.Execute(_context, _player);
    }
}
