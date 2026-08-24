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
        m1.Health.CurrentHealth = 60;
        world.Entities.Add(m1, player1);

        var m2 = UnitFactory.Create(
            TestDefinitionFactory.CreateMilitiaWithCombatStats(), player1.Id, new GridPosition(2, 1));
        m2.Health.TakeDamage(m2.Health.CurrentHealth);
        world.Entities.Add(m2, player1);

        var m3 = UnitFactory.Create(
            TestDefinitionFactory.CreateMilitiaWithCombatStats(), player1.Id, new GridPosition(3, 1));
        m3.Health.CurrentHealth = 60;
        world.Entities.Add(m3, player1);

        var m4 = UnitFactory.Create(
            TestDefinitionFactory.CreateMilitiaWithCombatStats(), player2.Id, new GridPosition(4, 1));
        m4.Health.CurrentHealth = 60;
        world.Entities.Add(m4, player2);

        var v1 = UnitFactory.Create(
            TestDefinitionFactory.CreateVillager(), player1.Id, new GridPosition(5, 1));
        v1.Health.CurrentHealth = 50;
        world.Entities.Add(v1, player1);

        Assert.Equal(0, UnitQueries.CountUnits(world, player1, "archer"));
        Assert.Equal(2, UnitQueries.CountUnits(world, player1, "militia"));
        Assert.Equal(1, UnitQueries.CountUnits(world, player2, "militia"));
        Assert.Equal(1, UnitQueries.CountUnits(world, player1, "villager"));
    }
}
