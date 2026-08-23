using RTSEngine.Core.Entities.States;
using RTSEngine.Core.Entities.Definitions;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.State;
using RTSEngine.Core.Systems;
using RTSEngine.Core.Actions;
using RTSEngine.Tests.TestHelpers;
namespace RTSEngine.Tests.Production;

public class ProductionSystemTests
{
    private readonly RuntimeContext _context;
    private readonly GameWorld _world;

    public ProductionSystemTests()
    {
        _world =
            TestWorldFactory.CreateWorldWithTwoPlayers();

        var unitDefinition =
            TestDefinitionFactory.CreateVillager();

        unitDefinition.ProductionTimeTicks = 3;

        var buildingDefinition =
            TestDefinitionFactory.CreateTownCenter();

        var unitRepository =
            new UnitDefinitionRepository(
            [
                unitDefinition
            ]);

        var buildingRepository =
            new BuildingDefinitionRepository(
            [
                buildingDefinition
            ]);

        _context = new RuntimeContext
        {
            World = _world,
            UnitRepository = unitRepository,
            BuildingRepository = buildingRepository
        };
    }


    [Fact]
    [Trait("Category", "Production")]
    public void Production_ShouldDecreaseRemainingTicks()
    {
        var building =
            BuildingFactory.Create(
                TestDefinitionFactory.CreateTownCenter(),
                1,
                new GridPosition(5,5));

        _world.AddEntity(building);

        building.Production.Add(
            new ProductionTask(
                "villager",
                3));

        ProductionSystem.Update(_context);

        Assert.Equal(
            2,
            building.Production.Current!.RemainingTicks);
    }

    [Fact]
    [Trait("Category", "Production")]
    public void Production_ShouldSpawnUnit_OnProductionSpawnPoint_WhenCompleted()
    {
        var building = BuildingFactory.Create(
                TestDefinitionFactory.CreateTownCenter(),
                1,
                new GridPosition(5,5));
        building.Production.SpawnPoint = new GridPosition(9,9);
        _world.AddEntity(building);

        var player = _world.GetPlayerById(1)!;
        PopulationActions.IncreaseCap(player, 5);
        PopulationActions.TryReservePopulation(player, 1);

        var spawnPoint = new GridPosition(9,5);
        building.Production.SpawnPoint = spawnPoint;
        building.Production.Add(
            new ProductionTask(
                "villager",
                3));

        RunProductionTicks(3);

        var unit =
            _world.Entities
            .OfType<Unit>()
            .Single();

        Assert.Equal(
            building.OwnerId,
            unit.OwnerId);
        Assert.Equal(
            spawnPoint,
            unit.Position);
    }
    private void RunProductionTicks(int ticks)
    {
        for(int i = 0; i < ticks; i++)
        {
            ProductionSystem.Update(_context);
        }
    }
}