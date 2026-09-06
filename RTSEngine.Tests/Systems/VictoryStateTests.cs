using RTSEngine.Core.State;
using RTSEngine.Core.Settings;
using RTSEngine.Core.Players;
using RTSEngine.Core.Entities.Definitions;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Tests.TestHelpers;

namespace RTSEngine.Tests.Systems;

public class VictoryStateTests
{
    [Fact]
    [Trait("Category", "Victory")]
    public void Conquest_ShouldSetWinner_WhenOnlyOnePlayerHasBuildings()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var player1 = world.GetPlayerById(1)!;
        var player2 = world.GetPlayerById(2)!;

        var definition = new BuildingDefinition
        {
            Id = "barracks",
            Name = "Barracks",
            MaxHealth = 100,
            Width = 2,
            Height = 2
        };

        var building = BuildingFactory.Create(definition, 1, new GridPosition(0, 0));
        building.IsCompleted = true;
        building.Health.CurrentHealth = building.Health.MaxHealth;
        world.Entities.Add(building, player1);

        var victory = new VictoryState();
        var condition = new VictoryCondition { Type = VictoryConditionType.Conquest };

        victory.Check(world.Players, condition);

        Assert.Equal(1, victory.WinnerPlayerId);
    }

    [Fact]
    [Trait("Category", "Victory")]
    public void Conquest_ShouldNotSetWinner_WhenMultiplePlayersHaveBuildings()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var player1 = world.GetPlayerById(1)!;
        var player2 = world.GetPlayerById(2)!;

        var definition = new BuildingDefinition
        {
            Id = "barracks",
            Name = "Barracks",
            MaxHealth = 100,
            Width = 2,
            Height = 2
        };

        var building1 = BuildingFactory.Create(definition, 1, new GridPosition(0, 0));
        building1.IsCompleted = true;
        building1.Health.CurrentHealth = building1.Health.MaxHealth;
        world.Entities.Add(building1, player1);

        var building2 = BuildingFactory.Create(definition, 2, new GridPosition(5, 5));
        building2.IsCompleted = true;
        building2.Health.CurrentHealth = building2.Health.MaxHealth;
        world.Entities.Add(building2, player2);

        var victory = new VictoryState();
        var condition = new VictoryCondition { Type = VictoryConditionType.Conquest };

        victory.Check(world.Players, condition);

        Assert.Null(victory.WinnerPlayerId);
    }

    [Fact]
    [Trait("Category", "Victory")]
    public void Conquest_ShouldNotSetWinner_WhenNoPlayerHasBuildings()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();

        var victory = new VictoryState();
        var condition = new VictoryCondition { Type = VictoryConditionType.Conquest };

        victory.Check(world.Players, condition);

        Assert.Null(victory.WinnerPlayerId);
    }

    [Fact]
    [Trait("Category", "Victory")]
    public void Conquest_ShouldNotOverwriteWinner_OnceSet()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var player1 = world.GetPlayerById(1)!;
        var player2 = world.GetPlayerById(2)!;

        var definition = new BuildingDefinition
        {
            Id = "barracks",
            Name = "Barracks",
            MaxHealth = 100,
            Width = 2,
            Height = 2
        };

        var building1 = BuildingFactory.Create(definition, 1, new GridPosition(0, 0));
        building1.IsCompleted = true;
        building1.Health.CurrentHealth = building1.Health.MaxHealth;
        world.Entities.Add(building1, player1);

        var victory = new VictoryState();
        var condition = new VictoryCondition { Type = VictoryConditionType.Conquest };

        victory.Check(world.Players, condition);
        Assert.Equal(1, victory.WinnerPlayerId);

        var building2 = BuildingFactory.Create(definition, 2, new GridPosition(5, 5));
        building2.IsCompleted = true;
        building2.Health.CurrentHealth = building2.Health.MaxHealth;
        world.Entities.Add(building2, player2);

        victory.Check(world.Players, condition);
        Assert.Equal(1, victory.WinnerPlayerId);
    }

    [Fact]
    [Trait("Category", "Victory")]
    public void ScoreLimit_ShouldSetWinner_WhenPlayerReachesTarget()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var player1 = world.GetPlayerById(1)!;
        player1.Score = 5000;

        var victory = new VictoryState();
        var condition = new VictoryCondition
        {
            Type = VictoryConditionType.ScoreLimit,
            ScoreTarget = 5000
        };

        victory.Check(world.Players, condition);

        Assert.Equal(1, victory.WinnerPlayerId);
    }

    [Fact]
    [Trait("Category", "Victory")]
    public void ScoreLimit_ShouldNotSetWinner_WhenNoPlayerReachesTarget()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var player1 = world.GetPlayerById(1)!;
        var player2 = world.GetPlayerById(2)!;
        player1.Score = 3000;
        player2.Score = 4000;

        var victory = new VictoryState();
        var condition = new VictoryCondition
        {
            Type = VictoryConditionType.ScoreLimit,
            ScoreTarget = 5000
        };

        victory.Check(world.Players, condition);

        Assert.Null(victory.WinnerPlayerId);
    }
}
