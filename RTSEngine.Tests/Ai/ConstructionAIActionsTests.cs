using RTSEngine.Core.AI.Actions;
using RTSEngine.Core.Entities.Buildings;
using RTSEngine.Core.Entities.Definitions;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Players;
using RTSEngine.Core.State;
using RTSEngine.Core.Systems;
using RTSEngine.Tests.TestHelpers;

namespace RTSEngine.Tests.AI.Actions;

public class ConstructionAIActionsTests
{
    private readonly RuntimeContext _context;
    private readonly GameWorld _world;
    private readonly Player _player;

    public ConstructionAIActionsTests()
    {
        _context = new RuntimeContext
        {
            World = TestWorldFactory.CreateWorldWithTwoPlayers(),
            UnitRepository = new UnitDefinitionRepository([]),
            BuildingRepository = new BuildingDefinitionRepository(
            [
                TestDefinitionFactory.CreateHouseWithCost(),
                TestDefinitionFactory.CreateTownCenter()
            ])
        };

        _world = _context.World;
        _player = _world.GetPlayerById(1)!;

        _player.Economy.Add(ResourceType.Wood, 1000);

        var townCenter = BuildingFactory.Create(
            TestDefinitionFactory.CreateTownCenter(),
            _player.Id,
            new GridPosition(20, 20));
        townCenter.IsCompleted = true;
        _world.AddEntity(townCenter);

    }

    [Fact]
    [Trait("Category", "AI")]
    public void RequestConstruction_ShouldCreateFoundation()
    {
        var villager = UnitFactory.Create(
            TestDefinitionFactory.CreateVillager(),
            _player.Id,
            new GridPosition(18, 20));

        _world.AddEntity(villager);
        var result = ConstructionAIActions.RequestConstruction(
            _context,
            _player,
            "house");

        // Assert
        Assert.True(result);

        var foundation = Assert.Single(
            _world.Entities.OfType<Building>(),
            building => !building.IsCompleted);

        Assert.Equal("house", foundation.Definition.Id);
        Assert.Equal(0, foundation.ConstructionProgress);
    }

    [Fact]
    [Trait("Category", "AI")]
    public void RequestConstruction_ShouldAssignBuilder()
    {
        var villager = UnitFactory.Create(
            TestDefinitionFactory.CreateVillager(),
            _player.Id,
            new GridPosition(18, 20));
        _world.AddEntity(villager);
        // Act
        ConstructionAIActions.RequestConstruction(
            _context,
            _player,
            "house");

        CommandSystem.Update(_world);

        // Assert
        Assert.Equal(
            UnitTask.Building,
            villager.CurrentTask);

        Assert.NotNull(villager.Build.BuildingId);
    }
}