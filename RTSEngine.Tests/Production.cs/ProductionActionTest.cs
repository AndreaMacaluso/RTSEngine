using RTSEngine.Core.Entities.Definitions;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Entities.States;
using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.State;
using RTSEngine.Core.Actions;
using RTSEngine.Core.Players;
using RTSEngine.Tests.TestHelpers;
using RTSEngine.Core.Entities.Buildings;

namespace RTSEngine.Tests.Production;

public class ProductionActionTests
{
    private readonly RuntimeContext _context;
    private readonly GameWorld _world;
    private readonly RTSEngine.Core.Entities.Buildings.Building _townCenter;
    private readonly Player _player;


    public ProductionActionTests()
    {
        _world = TestWorldFactory.CreateWorldWithTwoPlayers();

        var villager = TestDefinitionFactory.CreateVillager();
        villager.ProductionTimeTicks = 2;
       
        var townCenter = TestDefinitionFactory.CreateTownCenter();

        _context = new RuntimeContext
        {
            World = _world,

            UnitRepository = new UnitDefinitionRepository(
            [
                villager
            ]),

            BuildingRepository = new BuildingDefinitionRepository(
            [
                townCenter
            ])
        };

        _townCenter = BuildingFactory.Create(
            townCenter,
            ownerId: 1,
            position: new GridPosition(5,5));
        _townCenter.Production.SpawnPoint = new GridPosition(7,7);
        _world.AddEntity(_townCenter);

        _player = _world.GetPlayerById(1)!;
        PopulationActions.IncreaseCap(_player, 10);
        _player.Economy.Add(ResourceType.Food, 200);
    }


    [Fact]
    [Trait("Category", "Production")]
    public void ProduceOneTick_ShouldDecreaseProductionTime()
    {
        _townCenter.Production.Add(
            new ProductionTask(
                "villager",
                2));

        ProductionActions.ProduceOneTick(
            _context,
            _townCenter);

        Assert.Equal(
            1,
            _townCenter.Production.Current!.RemainingTicks);
    }


    [Fact]
    [Trait("Category", "Production")]
    public void ProduceOneTick_ShouldNotSpawnBeforeCompletion()
    {
        _townCenter.Production.Add(
            new ProductionTask(
                "villager",
                2));

        ProductionActions.ProduceOneTick(
            _context,
            _townCenter);

        Assert.Empty(
            _world.Entities.OfType<Unit>());
    }


    [Fact]
    [Trait("Category", "Production")]
    public void ProduceOneTick_ShouldSpawnUnit_WhenCompleted()
    {
        var player = _world.GetPlayerById(1)!;
        PopulationActions.IncreaseCap(player, 5);
        PopulationActions.TryReservePopulation(player, 1);

        _townCenter.Production.Add(
            new ProductionTask(
                "villager",
                1));

        ProductionActions.ProduceOneTick(
            _context,
            _townCenter);

        var unit =
            _world.Entities.OfType<Unit>()
            .FirstOrDefault();

        Assert.NotNull(unit);

        Assert.Equal(
            _townCenter.OwnerId,
            unit!.OwnerId);
    }


    [Fact]
    [Trait("Category", "Production")]
    public void ProduceOneTick_ShouldRemoveCompletedTask()
    {
        var player = _world.GetPlayerById(1)!;
        PopulationActions.IncreaseCap(player, 5);
        PopulationActions.TryReservePopulation(player, 1);

        _townCenter.Production.Add(
            new ProductionTask(
                "villager",
                1));

        ProductionActions.ProduceOneTick(
            _context,
            _townCenter);

        Assert.Null(
            _townCenter.Production.Current);
    }


    [Fact]
    [Trait("Category", "Production")]
    public void ProduceOneTick_ShouldDoNothing_WhenNoProductionExists()
    {
        ProductionActions.ProduceOneTick(
            _context,
            _townCenter);

        Assert.Empty(
            _world.Entities.OfType<Unit>());
    }


    [Fact]
    [Trait("Category", "Production")]
    public void TrainUnit_ShouldQueueProductionCommand()
    {
        var result =
            ProductionActions.TryTrainUnit(
                _context,
                _townCenter,
                "villager");

        Assert.True(result);

        Assert.Single(
            _world.PendingCommands);
    }


    [Fact]
    [Trait("Category", "Production")]
    public void TrainUnit_ShouldFail_WhenBuildingCannotProduceUnit()
    {
        _townCenter.Definition.Produces.Clear();

        var result =
            ProductionActions.TryTrainUnit(
                _context,
                _townCenter,
                "villager");

        Assert.False(result);

        Assert.Empty(
            _world.PendingCommands);
    }


    [Fact]
    [Trait("Category", "Production")]
    public void TrainUnit_ShouldFail_WhenUnitDoesNotExist()
    {
        var result =
            ProductionActions.TryTrainUnit(
                _context,
                _townCenter,
                "dragon");

        Assert.False(result);

        Assert.Empty(
            _world.PendingCommands);
    }
}