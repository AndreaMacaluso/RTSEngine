using RTSEngine.Core.AI.Decisions;
using RTSEngine.Core.Entities.Buildings;
using RTSEngine.Core.Entities.Definitions;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Players;
using RTSEngine.Core.State;
using RTSEngine.Tests.TestHelpers;

namespace RTSEngine.Tests.AI.Decisions;

public class ConstructionDecisionTests
{
    private readonly RuntimeContext _context;
    private readonly GameWorld _world;
    private readonly Player _player;

    public ConstructionDecisionTests()
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
    }

    [Fact]
    [Trait("Category", "AI")]
    public void Execute_ShouldStartHouseConstruction_WhenPopulationIsFull()
    {
        // Arrange
        _player.Population.Current = 5;
        _player.Population.Capacity = 5;
        _player.Economy.Add(ResourceType.Wood, 1000);

        var townCenter = BuildingFactory.Create(
            TestDefinitionFactory.CreateTownCenter(),
            _player.Id,
            new GridPosition(20, 20));
        townCenter.IsCompleted = true;
        _world.AddEntity(townCenter);

        var villager = UnitFactory.Create(
            TestDefinitionFactory.CreateVillager(),
            _player.Id,
            new GridPosition(18, 20));

        _world.AddEntity(villager);

        // Act
        ConstructionDecision.Execute(
            _context,
            _player);

        // Assert
        var foundation = Assert.Single(
            _world.Entities.OfType<Building>(),
            building => !building.IsCompleted);

        Assert.Equal("house", foundation.Definition.Id);
    }

   [Fact]
    [Trait("Category", "AI")]
    public void Execute_ShouldDoNothing_WhenPopulationIsAvailable()
    {
        // Arrange
        _player.Population.Current = 2;
        _player.Population.Capacity = 10;

        var townCenter = BuildingFactory.Create(
            TestDefinitionFactory.CreateTownCenter(),
            _player.Id,
            new GridPosition(20, 20));

        townCenter.IsCompleted = true;

        _world.AddEntity(townCenter);

        var villager = UnitFactory.Create(
            TestDefinitionFactory.CreateVillager(),
            _player.Id,
            new GridPosition(18, 20));

        _world.AddEntity(villager);

        // Act
        ConstructionDecision.Execute(
            _context,
            _player);

        // Assert
        Assert.DoesNotContain(
            _world.Entities.OfType<Building>(),
            b => b.Definition.Id == "house");
    }
}