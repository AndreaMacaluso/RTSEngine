using RTSEngine.Core.Commands;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Simulation;
using RTSEngine.Core.State;
using RTSEngine.Core.Systems;
using RTSEngine.Tests.TestHelpers;
using RTSEngine.Core.Entities.Definitions;

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
    public void Villager_ShouldSpawnAfterProductionTime()
    {
        var building =
            BuildingFactory.Create(
                TestDefinitionFactory.CreateTownCenter(),
                1,
                new GridPosition(3,3));
        building.Production.SpawnPoint = new GridPosition(7,7);
        _world.AddEntity(building);

        _world.AddCommand(
            new QueueProductionCommand
            (
                building.Id,
                1,
               "villager"
            ));

        var simulation =
            new SimulationRunner(_context);

        for(int i=0;i<2;i++)
        {
            simulation.Step();
        }

        var unit =
            _world.Entities
            .OfType<Unit>()
            .FirstOrDefault();


        Assert.NotNull(unit);
        Assert.Equal(1, unit.OwnerId);
    }
}