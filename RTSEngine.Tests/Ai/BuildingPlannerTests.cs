using RTSEngine.Core.AI.Planning;
using RTSEngine.Core.Entities.Definitions;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Players;
using RTSEngine.Core.State;
using RTSEngine.Tests.TestHelpers;

namespace RTSEngine.Tests.AI.Planning;

public class BuildingPlannerTests
{
    private readonly RuntimeContext _context;
    private readonly GameWorld _world;
    private readonly Player _player;

    public BuildingPlannerTests()
    {
        _context = new RuntimeContext
        {
            World = TestWorldFactory.CreateWorldWithTwoPlayers(),
            UnitRepository = new UnitDefinitionRepository([]),
            BuildingRepository = new BuildingDefinitionRepository([])
        };

        _world = _context.World;
        _player = _world.GetPlayerById(1)!;
    }

    [Fact]
    [Trait("Category", "AI")]
    public void FindBuildPosition_ShouldReturnNull_WhenTownCenterDoesNotExist()
    {
        var definition =
            TestDefinitionFactory.CreateHouseWithCost();

        var position =
            BuildingPlanner.FindBuildPosition(
                _world,
                _player,
                definition);

        Assert.Null(position);
    }

    [Fact]
    [Trait("Category", "AI")]
    public void FindBuildPosition_ShouldReturnPosition_WhenTownCenterExists()
    {
        var townCenter = BuildingFactory.Create(
            TestDefinitionFactory.CreateTownCenter(),
            _player.Id,
            new GridPosition(20, 20));
        townCenter.IsCompleted = true;
        _world.AddEntity(townCenter);

        var definition =
            TestDefinitionFactory.CreateHouseWithCost();

        var position =
            BuildingPlanner.FindBuildPosition(
                _world,
                _player,
                definition);

        Assert.NotNull(position);

        Assert.True(
            position.Value != townCenter.Position);
    }
}