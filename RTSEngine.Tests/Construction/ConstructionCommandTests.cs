using RTSEngine.Core.Commands;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Entities.States;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.State;
using RTSEngine.Core.Systems;
using RTSEngine.Tests.TestHelpers;

namespace RTSEngine.Tests.Construction;

public class ConstructionCommandTests
{
    [Fact]
    [Trait("Category", "Building")]
    public void BuildCommand_ShouldAssignBuildingTask()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();

        var unit = UnitFactory.Create(
            TestDefinitionFactory.CreateVillager(),
            1,
            new GridPosition(1, 1));

        world.AddEntity(unit);

        var building  = BuildingFactory.Create(
        TestDefinitionFactory.CreateHouse(),
        ownerId: 1,
        position: new GridPosition(1, 5));

        world.AddEntity(building);

        world.AddCommand(new BuildCommand
        {
            UnitIds = [unit.Id],
            BuildingId = building.Id
        });

        CommandSystem.Update(world);

        Assert.Equal(UnitTask.Building, unit.CurrentTask);
    }

    [Fact]
    [Trait("Category", "Building")]
    public void BuildCommand_ShouldAssignBuildingId()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();

        var unit = UnitFactory.Create(
            TestDefinitionFactory.CreateVillager(),
            1,
            new GridPosition(1, 1));

        world.AddEntity(unit);

        var building  = BuildingFactory.Create(
        TestDefinitionFactory.CreateHouse(),
        ownerId: 1,
        position: new GridPosition(1, 5));

        world.AddEntity(building);
        world.AddCommand(new BuildCommand
        {
            UnitIds = [unit.Id],
            BuildingId = building.Id
        });

        CommandSystem.Update(world);

        Assert.Equal(building.Id, unit.Build.BuildingId);
    }

    [Fact]
    [Trait("Category", "Building")]
    public void BuildCommand_ShouldAssignMovingPhase()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();

        var unit = UnitFactory.Create(
            TestDefinitionFactory.CreateVillager(),
            1,
            new GridPosition(1, 1));

        world.AddEntity(unit);

        var building  = BuildingFactory.Create(
        TestDefinitionFactory.CreateHouse(),
        ownerId: 1,
        position: new GridPosition(1, 5));

        world.AddEntity(building);

        world.AddCommand(new BuildCommand
        {
            UnitIds = [unit.Id],
            BuildingId = building.Id
        });

        CommandSystem.Update(world);

        Assert.Equal(
            BuildPhase.MovingToConstruction,
            unit.Build.Phase);
    }

    [Fact]
    [Trait("Category", "Building")]
    public void BuildCommand_ShouldGenerateMovementPath()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();

        var unit = UnitFactory.Create(
            TestDefinitionFactory.CreateVillager(),
            1,
            new GridPosition(1, 1));

        world.AddEntity(unit);

        var building  = BuildingFactory.Create(
        TestDefinitionFactory.CreateHouse(),
        ownerId: 1,
        position: new GridPosition(1, 5));

        world.AddEntity(building);

        world.AddCommand(new BuildCommand
        {
            UnitIds = [unit.Id],
            BuildingId = building.Id
        });

        CommandSystem.Update(world);

        Assert.NotEmpty(unit.Movement.PathQueue);
    }
}