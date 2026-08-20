using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Helpers;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.State;
using RTSEngine.Tests.TestHelpers;

namespace RTSEngine.Tests.Systems;

public class UnitQueriesTests
{
    [Fact]
    [Trait("Category", "UnitQueries")]
    public void CountUnits_ShouldFilterByOwnerAliveAndType()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var player1 = world.GetPlayerById(1)!;
        var player2 = world.GetPlayerById(2)!;

        var m1 = UnitFactory.Create(
            TestDefinitionFactory.CreateMilitiaWithCombatStats(), player1.Id, new GridPosition(1, 1));
        world.AddEntity(m1);

        var m2 = UnitFactory.Create(
            TestDefinitionFactory.CreateMilitiaWithCombatStats(), player1.Id, new GridPosition(2, 1));
        m2.TakeDamage(m2.CurrentHealth);
        world.AddEntity(m2);

        var m3 = UnitFactory.Create(
            TestDefinitionFactory.CreateMilitiaWithCombatStats(), player1.Id, new GridPosition(3, 1));
        world.AddEntity(m3);

        var m4 = UnitFactory.Create(
            TestDefinitionFactory.CreateMilitiaWithCombatStats(), player2.Id, new GridPosition(4, 1));
        world.AddEntity(m4);

        var v1 = UnitFactory.Create(
            TestDefinitionFactory.CreateVillager(), player1.Id, new GridPosition(5, 1));
        world.AddEntity(v1);

        Assert.Equal(0, UnitQueries.CountUnits(world, player1, "archer"));
        Assert.Equal(2, UnitQueries.CountUnits(world, player1, "militia"));
        Assert.Equal(1, UnitQueries.CountUnits(world, player2, "militia"));
        Assert.Equal(1, UnitQueries.CountUnits(world, player1, "villager"));
    }
}
