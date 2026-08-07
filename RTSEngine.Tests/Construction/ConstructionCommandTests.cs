using RTSEngine.Core.Commands;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Entities.States;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.State;
using RTSEngine.Core.Systems;
using RTSEngine.Tests.TestHelpers;
using RTSEngine.Core.Entities.Definitions;
using RTSEngine.Core.Players;

namespace RTSEngine.Tests.Construction;

public class ConstructionCommandTests
{
    private readonly RuntimeContext _context;
    private readonly GameWorld _world;
    private readonly Player _player;


    public ConstructionCommandTests()
    {
        _context = new RuntimeContext
        {
            World = TestWorldFactory.CreateWorldWithTwoPlayers(),

            UnitRepository =
            new UnitDefinitionRepository([]),

            BuildingRepository =
            new BuildingDefinitionRepository(
            [
                TestDefinitionFactory.CreateHouseWithCost(),
                TestDefinitionFactory.CreateTownCenter()
            ])
        };


        _world = _context.World;
        _player = _world.GetPlayerById(1)!;
    }

    [Fact]
    [Trait("Category", "Building")]
    public void BuildCommand_ShouldAssignConstructionTaskAndMovement()
    {
        var unit = UnitFactory.Create(
            TestDefinitionFactory.CreateVillager(),
            1,
            new GridPosition(1,1));

        _world.AddEntity(unit);

        var building = BuildingFactory.Create(
            TestDefinitionFactory.CreateHouse(),
            ownerId:1,
            position:new GridPosition(1,5));

        _world.AddEntity(building);


        _world.AddCommand(
            new BuildCommand
            {
                UnitIds = [unit.Id],
                BuildingId = building.Id
            });

        CommandSystem.Update(_context);

        Assert.Equal(
            UnitTask.Building,
            unit.CurrentTask);

        Assert.Equal(
            building.Id,
            unit.Build.BuildingId);

        Assert.Equal(
            BuildPhase.MovingToConstruction,
            unit.Build.Phase);

        Assert.NotEmpty(
            unit.Movement.PathQueue);
    }
}