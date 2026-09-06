using RTSEngine.Core.State;
using RTSEngine.Core.Systems;
using RTSEngine.Core.Players;
using RTSEngine.Core.Entities.Definitions;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Tests.TestHelpers;

namespace RTSEngine.Tests.Systems;

public class ScoreSystemTests
{
    [Fact]
    [Trait("Category", "Score")]
    public void Update_ShouldNotCalculate_WhenTickIsNotMultipleOf60()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var player = world.GetPlayerById(1)!;

        var definition = new UnitDefinition
        {
            Id = "villager",
            Name = "Villager",
            MaxHealth = 50,
            MovementSpeed = 1f,
            GatherCapacity = 20
        };

        var villager = UnitFactory.Create(definition, 1, new GridPosition(0, 0));
        world.Entities.Add(villager, player);

        world.AdvanceTick();

        ScoreSystem.Update(world);

        Assert.Equal(0, player.Score);
    }

    [Fact]
    [Trait("Category", "Score")]
    public void Update_ShouldCalculate_WhenTickIsMultipleOf60()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var player = world.GetPlayerById(1)!;

        var definition = new UnitDefinition
        {
            Id = "villager",
            Name = "Villager",
            MaxHealth = 50,
            MovementSpeed = 1f,
            GatherCapacity = 20
        };

        var villager = UnitFactory.Create(definition, 1, new GridPosition(0, 0));
        world.Entities.Add(villager, player);

        for (int i = 0; i < 60; i++)
        {
            world.AdvanceTick();
        }

        ScoreSystem.Update(world);

        Assert.Equal(1, player.Score);
    }

    [Fact]
    [Trait("Category", "Score")]
    public void Score_ShouldCountVillagersAs1Point()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var player = world.GetPlayerById(1)!;

        var definition = new UnitDefinition
        {
            Id = "villager",
            Name = "Villager",
            MaxHealth = 50,
            MovementSpeed = 1f,
            GatherCapacity = 20
        };

        for (int i = 0; i < 3; i++)
        {
            var villager = UnitFactory.Create(definition, 1, new GridPosition(i, 0));
            world.Entities.Add(villager, player);
        }

        SetTickTo60(world);
        ScoreSystem.Update(world);

        Assert.Equal(3, player.Score);
    }

    [Fact]
    [Trait("Category", "Score")]
    public void Score_ShouldCountMilitaryAs2Points()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var player = world.GetPlayerById(1)!;

        var definition = new UnitDefinition
        {
            Id = "militia",
            Name = "Militia",
            MaxHealth = 60,
            MovementSpeed = 1f
        };

        for (int i = 0; i < 2; i++)
        {
            var militia = UnitFactory.Create(definition, 1, new GridPosition(i, 0));
            world.Entities.Add(militia, player);
        }

        SetTickTo60(world);
        ScoreSystem.Update(world);

        Assert.Equal(4, player.Score);
    }

    [Fact]
    [Trait("Category", "Score")]
    public void Score_ShouldCountBuildingsAs3Points()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var player = world.GetPlayerById(1)!;

        var definition = new BuildingDefinition
        {
            Id = "barracks",
            Name = "Barracks",
            MaxHealth = 100,
            Width = 2,
            Height = 2
        };

        for (int i = 0; i < 2; i++)
        {
            var building = BuildingFactory.Create(definition, 1, new GridPosition(i * 3, 0));
            building.IsCompleted = true;
            building.Health.CurrentHealth = building.Health.MaxHealth;
            world.Entities.Add(building, player);
        }

        SetTickTo60(world);
        ScoreSystem.Update(world);

        Assert.Equal(6, player.Score);
    }

    [Fact]
    [Trait("Category", "Score")]
    public void Score_ShouldCountResourcesDividedBy10()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var player = world.GetPlayerById(1)!;

        player.Economy.Add(ResourceType.Wood, 30);
        player.Economy.Add(ResourceType.Food, 20);

        SetTickTo60(world);
        ScoreSystem.Update(world);

        Assert.Equal(5, player.Score);
    }

    [Fact]
    [Trait("Category", "Score")]
    public void Score_ShouldIgnoreDeadUnits()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var player = world.GetPlayerById(1)!;

        var definition = new UnitDefinition
        {
            Id = "villager",
            Name = "Villager",
            MaxHealth = 50,
            MovementSpeed = 1f,
            GatherCapacity = 20
        };

        var villager = UnitFactory.Create(definition, 1, new GridPosition(0, 0));
        world.Entities.Add(villager, player);
        villager.Health.TakeDamage(100);

        SetTickTo60(world);
        ScoreSystem.Update(world);

        Assert.Equal(0, player.Score);
    }

    [Fact]
    [Trait("Category", "Score")]
    public void Score_ShouldIgnoreDeadBuildings()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var player = world.GetPlayerById(1)!;

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
        world.Entities.Add(building, player);
        building.Health.TakeDamage(200);

        SetTickTo60(world);
        ScoreSystem.Update(world);

        Assert.Equal(0, player.Score);
    }

    [Fact]
    [Trait("Category", "Score")]
    public void Score_ShouldSumAllComponents()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var player = world.GetPlayerById(1)!;

        var villagerDef = new UnitDefinition
        {
            Id = "villager",
            Name = "Villager",
            MaxHealth = 50,
            MovementSpeed = 1f,
            GatherCapacity = 20
        };

        var militiaDef = new UnitDefinition
        {
            Id = "militia",
            Name = "Militia",
            MaxHealth = 60,
            MovementSpeed = 1f
        };

        var buildingDef = new BuildingDefinition
        {
            Id = "barracks",
            Name = "Barracks",
            MaxHealth = 100,
            Width = 2,
            Height = 2
        };

        var villager = UnitFactory.Create(villagerDef, 1, new GridPosition(0, 0));
        world.Entities.Add(villager, player);

        var militia = UnitFactory.Create(militiaDef, 1, new GridPosition(1, 0));
        world.Entities.Add(militia, player);

        var building = BuildingFactory.Create(buildingDef, 1, new GridPosition(5, 0));
        building.IsCompleted = true;
        building.Health.CurrentHealth = building.Health.MaxHealth;
        world.Entities.Add(building, player);

        player.Economy.Add(ResourceType.Wood, 100);

        SetTickTo60(world);
        ScoreSystem.Update(world);

        int expected = 1 + 2 + 3 + 10;
        Assert.Equal(expected, player.Score);
    }

    private static void SetTickTo60(GameWorld world)
    {
        for (int i = 0; i < 60; i++)
        {
            world.AdvanceTick();
        }
    }
}
