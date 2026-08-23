using RTSEngine.Core.Entities.States;

namespace RTSEngine.Tests.Production;

public class ProductionStateTests
{
    [Fact]
    [Trait("Category", "Production")]
    public void Add_ShouldSetCurrentProductionTask()
    {
        var state = new ProductionState();

        var task = new ProductionTask(
            "villager",
            10);

        state.Add(task);

        Assert.NotNull(state.Current);
        Assert.Equal("villager", state.Current!.ProductId);
    }


    [Fact]
    [Trait("Category", "Production")]
    public void RemoveCurrent_ShouldRemoveFirstTask()
    {
        var state = new ProductionState();

        state.Add(
            new ProductionTask(
                "villager",
                10));

        state.Add(
            new ProductionTask(
                "militia",
                20));


        state.RemoveCurrent();


        Assert.NotNull(state.Current);
        Assert.Equal("militia", state.Current!.ProductId);
    }


    [Fact]
    [Trait("Category", "Production")]
    public void IsIdle_ShouldBeTrue_WhenQueueIsEmpty()
    {
        var state = new ProductionState();

        Assert.False(state.IsProducing);
    }


    [Fact]
    [Trait("Category", "Production")]
    public void IsIdle_ShouldBeFalse_WhenProductionExists()
    {
        var state = new ProductionState();

        state.Add(
            new ProductionTask(
                "villager",
                10));


        Assert.True(state.IsProducing);
    }
}