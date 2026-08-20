using RTSEngine.Core.AI;

namespace RTSEngine.Tests.AI.Brains;

public class BrainActionsTests
{
    [Theory]
    [Trait("Category", "AI")]
    [Trait("Category", "Brain")]
    [InlineData(BrainActions.TrainVillager, "villager")]
    [InlineData(BrainActions.TrainMilitia, "militia")]
    [InlineData(BrainActions.BuildBarracks, "barracks")]
    [InlineData(BrainActions.BuildHouse, "house")]
    public void NeedsDefinition_ShouldReturnTrue_ForUnitAndBuildingActions(string action, string expectedDef)
    {
        Assert.True(BrainActions.NeedsDefinition(action));
        Assert.Equal(expectedDef, BrainActions.GetDefinition(action));
    }

    [Theory]
    [Trait("Category", "AI")]
    [Trait("Category", "Brain")]
    [InlineData(BrainActions.None)]
    [InlineData(BrainActions.AssignGatherers)]
    [InlineData(BrainActions.EngageEnemies)]
    public void NeedsDefinition_ShouldReturnFalse_ForNonDefinitionActions(string action)
    {
        Assert.False(BrainActions.NeedsDefinition(action));
        Assert.Null(BrainActions.GetDefinition(action));
    }

    [Fact]
    [Trait("Category", "AI")]
    [Trait("Category", "Brain")]
    public void AllActions_ShouldBeUnique()
    {
        var actions = new[]
        {
            BrainActions.None,
            BrainActions.TrainVillager,
            BrainActions.TrainMilitia,
            BrainActions.BuildBarracks,
            BrainActions.BuildHouse,
            BrainActions.AssignGatherers,
            BrainActions.EngageEnemies
        };

        Assert.Equal(actions.Length, actions.Distinct().Count());
    }
}
