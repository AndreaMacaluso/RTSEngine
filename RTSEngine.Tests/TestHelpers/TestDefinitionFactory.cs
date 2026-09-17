using RTSEngine.Core.Entities.Definitions;
using RTSEngine.Core.Helpers;
using RTSEngine.Core.Map.Runtime;

namespace RTSEngine.Tests.TestHelpers;

public static class TestDefinitionFactory
{
    public static UnitDefinition CreateVillager(
        FixedPoint movementSpeed = default,
        int gatherCapacity = 20)
    {
        if (movementSpeed == default) movementSpeed = FixedPoint.One;

        return new UnitDefinition
        {
            Id = "villager",
            Name = "Villager",
            MaxHealth = 50,
            MovementSpeed = movementSpeed,
            GatherCapacity = gatherCapacity,
            BuildableBuildings = [
                "town_center",
                "house"
                ],
            Costs = [new(ResourceType.Food,50)]
        };
    }
     public static UnitDefinition CreateMilitia(
        FixedPoint movementSpeed = default)
    {
        if (movementSpeed == default) movementSpeed = FixedPoint.One;

        return new UnitDefinition
        {
            Id = "militia",
            Name = "Militia",
            MaxHealth = 70,
            MovementSpeed = movementSpeed,    
        };
    }

    public static UnitDefinition CreateMilitiaWithCombatStats(
        FixedPoint movementSpeed = default,
        int meleeAttack = 6,
        int attackRange = 1,
        int attackCooldownTicks = 4)
    {
        if (movementSpeed == default) movementSpeed = FixedPoint.One;

        return new UnitDefinition
        {
            Id = "militia",
            Name = "Militia",
            MaxHealth = 60,
            MovementSpeed = movementSpeed,
            MeleeAttack = meleeAttack,
            AttackRange = attackRange,
            AttackCooldownTicks = attackCooldownTicks,
            Costs = [new(ResourceType.Food, 50)]
        };
    }

    public static BuildingDefinition CreateTownCenter()
    {
        return new BuildingDefinition
        {
            Id = "town_center",
            Name = "towncenter",
            Width = 3,
            Height = 3,
            MaxHealth = 1000,
            PopulationBonus = 5,
            AcceptedResources = [
                ResourceType.Wood,
            ],
            Produces = ["villager"]
        };

    }

    public static BuildingDefinition CreateBarracks()
    {
        return new BuildingDefinition
        {
            Id = "barracks",
            Name = "Barracks",
            MaxHealth = 100,
            Width = 2,
            Height = 2,
            Produces = ["militia"],
            Costs = [new(ResourceType.Wood, 50)]
        };
    }

    public static BuildingDefinition CreateHouse()
    {
        return new BuildingDefinition
        {
            Id = "house",
            Name = "House",
            MaxHealth = 100,
            Width = 2,
            Height = 2,
            PopulationBonus = 5,
            BuildTimeTicks = 10
        };

    }

    public static BuildingDefinition CreateHouseWithCost()
    {
        return new BuildingDefinition
        {
            Id = "house",
            Name = "House",
            BuildTimeTicks = 10,
            MaxHealth = 100,
            Width = 2,
            Height = 2,
            PopulationBonus = 5,
            Costs =
            [
                new(ResourceType.Wood, 100)
            ]
        };
    }
}