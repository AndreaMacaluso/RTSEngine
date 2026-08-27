using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Systems.Pathfinding;
using RTSEngine.Tests.TestHelpers;

namespace RTSEngine.Tests.Systems;

public class PathSystemTests
{
    private static readonly AStarPathFinder PathFinder = new(
        new GroundMovementFilter());

    [Fact]
    [Trait("Category", "Movement")]
    public void GeneratePath_ShouldCreateExpectedStepCountAndReachTarget()
    {
        var world = TestWorldFactory.CreateWorld();
        var path = PathFinder.FindPath(
            world,
            new GridPosition(1,1),
            new GridPosition(5,5));

        Assert.Equal(4, path.Count);
        Assert.NotEmpty(path);
        Assert.Equal(
            new GridPosition(5, 5),
            path.Last());
    }
}
