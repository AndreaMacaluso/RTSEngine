using RTSEngine.Core.Commands;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Simulation;
using RTSEngine.Core.State;
using RTSEngine.Core.Systems;
using RTSEngine.Tests.TestHelpers;
using RTSEngine.Core.Entities.Definitions;
using RTSEngine.Core.Actions;

namespace RTSEngine.Tests.Production;


public class UnitProductionFlowTests
{

    private readonly RuntimeContext _context;
    private readonly GameWorld _world;


    public UnitProductionFlowTests()
    {
        _world =
            TestWorldFactory.CreateWorldWithTwoPlayers();


        var villager =
            TestDefinitionFactory.CreateVillager();

        villager.ProductionTimeTicks = 2;


        var townCenter =
            TestDefinitionFactory.CreateTownCenter();

        _context = new RuntimeContext
        {
            World = _world,

            UnitRepository =
            new UnitDefinitionRepository(
            [
                villager
            ]),

            BuildingRepository =
            new BuildingDefinitionRepository(
            [
                townCenter
            ])
        };
    }


    [Fact]
    [Trait("Category", "Production")]
    public void QueueCommand_ShouldCreateProductionTask()
    {
        var building =
            BuildingFactory.Create(
                TestDefinitionFactory.CreateTownCenter(),
                1,
                new GridPosition(3,3));


        _world.AddEntity(building);


        _world.AddCommand(
            new QueueProductionCommand
            (
                building.Id,
                1,
               "villager"
            ));


        CommandSystem.Update(_context);


        Assert.NotNull(
            building.Production.Current);
    }



    [Fact]
    [Trait("Category", "Production")]
    public void TryTrainUnit_SucceedsWhenResourcesAndPopulationAllow()
    {
        var building =
            BuildingFactory.Create(
                TestDefinitionFactory.CreateTownCenter(),
                1,
                new GridPosition(3, 3));
        _world.AddEntity(building);

        var player = _world.GetPlayerById(1)!;
        player.Economy.Add(ResourceType.Food, 50);
        player.Population.Capacity = 10;

        var result = ProductionActions.TryTrainUnit(
            _context,
            building,
            "villager");

        Assert.True(result);
        var pending = _world.PendingCommands.ToArray();
        Assert.Single(pending);
        Assert.IsType<QueueProductionCommand>(pending[0]);
        Assert.Equal(0, player.Economy.Get(ResourceType.Food));
        Assert.Equal(1, player.Population.Reserved);
    }

    [Fact]
    [Trait("Category", "Production")]
    public void TryTrainUnit_FailsWhenInsufficientResources()
    {
        var building =
            BuildingFactory.Create(
                TestDefinitionFactory.CreateTownCenter(),
                1,
                new GridPosition(3, 3));
        _world.AddEntity(building);

        var player = _world.GetPlayerById(1)!;
        player.Population.Capacity = 10;

        var result = ProductionActions.TryTrainUnit(
            _context,
            building,
            "villager");

        Assert.False(result);
        Assert.Null(building.Production.Current);
        Assert.Empty(_world.PendingCommands);
        Assert.Equal(0, player.Population.Reserved);
    }
}