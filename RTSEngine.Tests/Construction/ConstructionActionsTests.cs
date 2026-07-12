using RTSEngine.Core.Commands;
using RTSEngine.Core.Entities.Resources;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.State;
using RTSEngine.Core.Actions;
using RTSEngine.Tests.TestHelpers;
using RTSEngine.Core.Entities.States;

namespace RTSEngine.Tests.Construction;

public class ConstructionActionsTests
{
    [Fact]
    [Trait("Category", "Building")]
    public void BuildOneTick_ShouldIncreaseProgress()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();

        var unit = UnitFactory.Create(
                TestDefinitionFactory.CreateVillager(),
                1,
                new GridPosition(1, 1));

        var building  = BuildingFactory.Create(
        TestDefinitionFactory.CreateHouse(),
        ownerId: 1,
        position: new GridPosition(1, 5));

        world.AddEntity(building);

        unit.Build.BuildingId = building.Id;

        ConstructionActions.BuildOneTick(world, unit);

        Assert.Equal(
            1,
            building.ConstructionProgress);
    }

    [Fact]
    [Trait("Category", "Building")]
    public void CompleteConstruction_ShouldCompleteBuilding()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();

        var unit = UnitFactory.Create(
                TestDefinitionFactory.CreateVillager(),
                1,
                new GridPosition(1, 1));
        var building  = BuildingFactory.Create(
        TestDefinitionFactory.CreateHouse(),
        ownerId: 1,
        position: new GridPosition(1, 5));

        world.AddEntity(building);

        unit.Build.BuildingId = building.Id;

        ConstructionActions.CompleteConstruction(
            world,
            unit);

        Assert.True(building.IsCompleted);
    }

    [Fact]
    [Trait("Category", "Building")]
    public void StopBuilding_ShouldClearBuilderState()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();

        var unit = UnitFactory.Create(
                TestDefinitionFactory.CreateVillager(),
                1,
                new GridPosition(1, 1));

        unit.Build.BuildingId = 10;
        unit.Build.Phase = BuildPhase.Constructing;
        unit.CurrentTask = UnitTask.Building;

        ConstructionActions.StopBuilding(unit);

        Assert.Null(unit.Build.BuildingId);
        Assert.Equal(BuildPhase.None, unit.Build.Phase);
        Assert.Equal(UnitTask.Idle, unit.CurrentTask);
    }
}