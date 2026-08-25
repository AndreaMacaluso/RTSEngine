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
    public void GeneratePath_ShouldCreateExpectedStepCount()
    {
        var world = TestWorldFactory.CreateWorld();
        var path = PathFinder.FindPath(
            world,
            new GridPosition(1,1),
            new GridPosition(5,5));

        Assert.Equal(4, path.Count);
    }

    [Fact]
    public void GeneratePath_ShouldReachTarget()
    {
        var world = TestWorldFactory.CreateWorld();

        var start = new GridPosition(1, 1);
        var target = new GridPosition(5, 5);

        var path = PathFinder.FindPath(
            world,
            start,
            target);

        Assert.NotEmpty(path);

        Assert.Equal(
            target,
            path.Last());
    }
}
