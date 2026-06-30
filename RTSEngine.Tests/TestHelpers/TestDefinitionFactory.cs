using RTSEngine.Core.Entities.Definitions;

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
}