using RTSEngine.Core.Entities.States;
using RTSEngine.Core.Systems;
using RTSEngine.Tests.TestHelpers;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.State;
using RTSEngine.Core.Commands;

namespace RTSEngine.Tests.Construction;

public class ConstructionSystemTests
{
    [Fact]
    [Trait("Category", "Building")]
    public void Update_ShouldSwitchToConstructing_WhenDestinationReached()
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
        position: new GridPosition(1, 2));

        world.AddEntity(building);

        unit.Build.BuildingId = building.Id;

        unit.Build.Phase =
            BuildPhase.MovingToConstruction;

        unit.Movement.PathQueue.Clear();

        ConstructionSystem.Update(world);

        Assert.Equal(
            BuildPhase.Constructing,
            unit.Build.Phase);
    }

    [Fact]
    [Trait("Category", "Building")]
    public void Update_ShouldCompleteBuilding_WhenProgressFinishes()
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

        unit.Build.BuildingId = building.Id;
        unit.Build.Phase = BuildPhase.Constructing;
        Assert.True(unit.Definition.CanBuild);
        building.ConstructionProgress =
            building.Definition.BuildTimeTicks - 1;

        ConstructionSystem.Update(world);

        Assert.True(building.IsCompleted);
        Assert.Equal(UnitTask.Idle, unit.CurrentTask);
        Assert.Equal(BuildPhase.None, unit.Build.Phase);
    }

    [Fact]
    [Trait("Category", "Building")]
    [Trait("Category", "BuildingLoop")]
    public void Builder_Should_ConstructBuilding_FromStartToFinish()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();

        var villager = UnitFactory.Create(
            TestDefinitionFactory.CreateVillager(),
            ownerId: 1,
            position: new GridPosition(1, 1));

        world.AddEntity(villager);

        var building = BuildingFactory.Create(
            TestDefinitionFactory.CreateHouse(),
            ownerId: 1,
            position: new GridPosition(5, 5));

        world.AddEntity(building);

        world.AddCommand(new BuildCommand
        {
            UnitIds = [villager.Id],
            BuildingId = building.Id
        });

        SimulationTestHelper.RunTicks(world, 50);
        Assert.Equal(
            UnitTask.Idle,
            villager.CurrentTask);
        
        Assert.Equal(
            BuildPhase.None,
            villager.Build.Phase);
        Assert.Null(villager.Build.BuildingId);
        Assert.True(building.IsCompleted);

        Assert.Equal(
            building.Definition.MaxHealth,
            building.CurrentHealth);
    }
}