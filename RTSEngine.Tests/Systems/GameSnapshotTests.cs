using RTSEngine.Core.State;
using RTSEngine.Core.Entities.Buildings;
using RTSEngine.Core.Entities.Resources;
using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Players;
using RTSEngine.Tests.TestHelpers;
using Xunit;

namespace RTSEngine.Tests.Systems;

[Trait("Category", "Snapshot")]
public class GameSnapshotTests
{
    [Fact]
    public void Capture_ShouldIncludeAllEntities_WhenWorldHasEntities()
    {
        var world = TestWorldFactory.CreateWorld();
        var player = new Player(1, "Red", ConsoleColor.Red, PlayerControllerType.Human);
        world.AddPlayer(player);

        var villagerDefinition = TestDefinitionFactory.CreateVillager();
        var villager = new Unit(1, new GridPosition(5, 5), villagerDefinition);
        world.Entities.Add(villager, player);

        var houseDefinition = TestDefinitionFactory.CreateHouse();
        var house = new Building(1, new GridPosition(10, 10), houseDefinition);
        house.IsCompleted = true;
        world.Entities.Add(house, player);

        var snapshot = GameSnapshot.Capture(world);

        Assert.Equal(0, snapshot.Tick);
        Assert.Equal("Running", snapshot.State);
        Assert.Single(snapshot.Units);
        Assert.Single(snapshot.Buildings);
        Assert.Single(snapshot.Players);
    }

    [Fact]
    public void Serialize_ShouldProduceValidJson_WhenCalled()
    {
        var world = TestWorldFactory.CreateWorld();
        var player = new Player(1, "Red", ConsoleColor.Red, PlayerControllerType.Human);
        world.AddPlayer(player);

        var snapshot = GameSnapshot.Capture(world);
        var json = snapshot.Serialize();

        Assert.False(string.IsNullOrEmpty(json));
        Assert.Contains("\"tick\":0", json);
        Assert.Contains("\"state\":\"Running\"", json);
    }

    [Fact]
    public void Deserialize_ShouldRestoreSnapshot_WhenCalledWithValidJson()
    {
        var world = TestWorldFactory.CreateWorld();
        var player = new Player(1, "Red", ConsoleColor.Red, PlayerControllerType.Human);
        world.AddPlayer(player);

        var original = GameSnapshot.Capture(world);
        var json = original.Serialize();
        var restored = GameSnapshot.Deserialize(json);

        Assert.NotNull(restored);
        Assert.Equal(original.Tick, restored!.Tick);
        Assert.Equal(original.State, restored.State);
        Assert.Equal(original.Units.Length, restored.Units.Length);
        Assert.Equal(original.Buildings.Length, restored.Buildings.Length);
        Assert.Equal(original.Players.Length, restored.Players.Length);
    }

    [Fact]
    public void Capture_ShouldIncludeDeadUnits_WhenUnitIsDead()
    {
        var world = TestWorldFactory.CreateWorld();
        var player = new Player(1, "Red", ConsoleColor.Red, PlayerControllerType.Human);
        world.AddPlayer(player);

        var villagerDefinition = TestDefinitionFactory.CreateVillager();
        var villager = new Unit(1, new GridPosition(5, 5), villagerDefinition);
        world.Entities.Add(villager, player);

        villager.Health.TakeDamage(villager.Health.MaxHealth);

        var snapshot = GameSnapshot.Capture(world);

        Assert.Single(snapshot.Units);
    }

    [Fact]
    public void Roundtrip_ShouldPreserveData_WhenSerializeAndDeserialize()
    {
        var world = TestWorldFactory.CreateWorld();
        var player = new Player(1, "Red", ConsoleColor.Red, PlayerControllerType.Human);
        player.Economy.Add(ResourceType.Wood, 100);
        player.Economy.Add(ResourceType.Food, 200);
        world.AddPlayer(player);

        var villagerDefinition = TestDefinitionFactory.CreateVillager();
        var villager = new Unit(1, new GridPosition(5, 5), villagerDefinition);
        world.Entities.Add(villager, player);

        var snapshot = GameSnapshot.Capture(world);
        var json = snapshot.Serialize();
        var restored = GameSnapshot.Deserialize(json);

        Assert.NotNull(restored);
        Assert.Single(restored!.Units);
        Assert.Single(restored.Players);
    }
}
