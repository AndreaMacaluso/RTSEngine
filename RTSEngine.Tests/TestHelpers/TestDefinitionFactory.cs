using RTSEngine.Core.Entities.Definitions;
using RTSEngine.Core.Map.Runtime;

namespace RTSEngine.Tests.TestHelpers;

public static class TestDefinitionFactory
{
    public static UnitDefinition CreateVillager(
        float movementSpeed = 1f,
        int gatherCapacity = 20)
    {
        return new UnitDefinition
        {
            Id = "villager",
            Name = "Villager",
            MaxHealth = 50,
            MovementSpeed = movementSpeed,
            GatherCapacity = gatherCapacity,
            BuildableBuildings = [
                "towncenter",
                "house"
                ]
        };
    }
     public static UnitDefinition CreateMilitia(
        float movementSpeed = 1f)
    {
        return new UnitDefinition
        {
            Id = "militia",
            Name = "Militia",
            MaxHealth = 70,
            MovementSpeed = movementSpeed,    
        };
    }

    public static BuildingDefinition CreateTownCenter()
    {
        return new BuildingDefinition
        {
            Id = "towncenter",
            Name = "towncenter",
            AcceptedResources = [
                ResourceType.Wood,
            ]  
        };

    }
        public static BuildingDefinition CreateHouse()
    {
        return new BuildingDefinition
        {
            Id = "house",
            Name = "House",
            BuildTimeTicks = 10
        };

    }
}