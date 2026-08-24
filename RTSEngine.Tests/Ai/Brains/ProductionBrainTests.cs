using RTSEngine.Core.AI;
using RTSEngine.Core.AI.Brains;
using RTSEngine.Core.Entities.Buildings;
using RTSEngine.Core.Entities.Definitions;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Players;
using RTSEngine.Core.State;
using RTSEngine.Core.Systems;
using RTSEngine.Tests.TestHelpers;

namespace RTSEngine.Tests.AI.Brains;

public class ProductionBrainTests
{
    private readonly RuntimeContext _context;
    private readonly GameWorld _world;
    private readonly Player _player;

    public ProductionBrainTests()
    {
        _context = new RuntimeContext
        {
            World = TestWorldFactory.CreateWorldWithTwoPlayers(),
            UnitRepository = new UnitDefinitionRepository([
                TestDefinitionFactory.CreateVillager(),
                TestDefinitionFactory.CreateMilitiaWithCombatStats()
            ]),
            BuildingRepository = new BuildingDefinitionRepository([
                TestDefinitionFactory.CreateTownCenter(),
                TestDefinitionFactory.CreateBarracks()
            ])
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
    public void ShouldTrainVillager_WhenPopBelowTarget()
    {
        var tc = CreateCompletedBuilding(
            TestDefinitionFactory.CreateTownCenter(),
            _player.Id,
            new GridPosition(0, 0));
        _world.Entities.Add(tc, _player);

        _player.Population.Current = 0;
        _player.Population.Capacity = 5;
        _player.Economy.Add(ResourceType.Food, 200);

        var brain = new ProductionBrain();
        brain.Execute(_context, _player);
        CommandSystem.Update(_context);

        Assert.True(tc.Production.IsProducing);
    }

    [Fact]
    [Trait("Category", "AI")]
    [Trait("Category", "Brain")]
    public void ShouldNotTrainVillager_WhenNoFood()
    {
        var tc = CreateCompletedBuilding(
            TestDefinitionFactory.CreateTownCenter(),
            _player.Id,
            new GridPosition(0, 0));
        _world.Entities.Add(tc, _player);

        _player.Population.Current = 0;
        _player.Population.Capacity = 5;

        var brain = new ProductionBrain();
        brain.Execute(_context, _player);
        CommandSystem.Update(_context);

        Assert.False(tc.Production.IsProducing);
    }

    [Fact]
    [Trait("Category", "AI")]
    [Trait("Category", "Brain")]
    public void ShouldTrainMilitia_WhenPopReachedTarget()
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
        _player.Economy.Add(ResourceType.Food, 200);

        var brain = new ProductionBrain();
        brain.Execute(_context, _player);
        CommandSystem.Update(_context);

        Assert.True(barracks.Production.IsProducing);
    }

    [Fact]
    [Trait("Category", "AI")]
    [Trait("Category", "Brain")]
    public void ShouldNotTrainMilitia_WhenMaxReached()
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

        for (int i = 0; i < GameConfig.TargetMilitiaCount; i++)
        {
            var militia = UnitFactory.Create(
                TestDefinitionFactory.CreateMilitiaWithCombatStats(),
                _player.Id,
                new GridPosition(8 + i, 0));
            militia.Health.CurrentHealth = 60;
            _world.Entities.Add(militia, _player);
        }

        _player.Population.Current = 15;
        _player.Population.Capacity = 20;
        _player.Economy.Add(ResourceType.Food, 200);

        var brain = new ProductionBrain();
        brain.Execute(_context, _player);
        CommandSystem.Update(_context);

        Assert.False(barracks.Production.IsProducing);
    }

    [Fact]
    [Trait("Category", "AI")]
    [Trait("Category", "Brain")]
    public void ShouldReturnNone_WhenNoTownCenter()
    {
        _player.Population.Current = 0;
        _player.Population.Capacity = 5;
        _player.Economy.Add(ResourceType.Food, 200);

        var brain = new ProductionBrain();
        brain.Execute(_context, _player);
    }
}
