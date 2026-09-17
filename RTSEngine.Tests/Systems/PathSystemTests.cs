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

    [Fact]
    [Trait("Category", "Pathfinding")]
    public void FindPath_ShouldReturnIdenticalPaths_WhenRunMultipleTimes()
    {
        var world = TestWorldFactory.CreateWorld();
        var start = new GridPosition(1, 1);
        var target = new GridPosition(10, 10);

        var paths = new List<List<GridPosition>>();
        for (int i = 0; i < 10; i++)
        {
            var path = PathFinder.FindPath(world, start, target);
            paths.Add(path.ToList());
        }

        for (int i = 1; i < paths.Count; i++)
        {
            Assert.Equal(paths[0].Count, paths[i].Count);
            for (int j = 0; j < paths[0].Count; j++)
            {
                Assert.Equal(paths[0][j], paths[i][j]);
            }
        }
    }

    [Fact]
    [Trait("Category", "Pathfinding")]
    public void FindPath_ShouldReturnIdenticalPaths_ForDifferentStartTargets()
    {
        var world = TestWorldFactory.CreateWorld();

        var testCases = new[]
        {
            (new GridPosition(1, 1), new GridPosition(5, 5)),
            (new GridPosition(1, 1), new GridPosition(10, 10)),
            (new GridPosition(5, 5), new GridPosition(1, 1)),
            (new GridPosition(1, 10), new GridPosition(10, 1)),
        };

        foreach (var (start, target) in testCases)
        {
            var paths = new List<List<GridPosition>>();
            for (int i = 0; i < 5; i++)
            {
                var path = PathFinder.FindPath(world, start, target);
                paths.Add(path.ToList());
            }

            for (int i = 1; i < paths.Count; i++)
            {
                Assert.Equal(paths[0].Count, paths[i].Count);
                for (int j = 0; j < paths[0].Count; j++)
                {
                    Assert.Equal(paths[0][j], paths[i][j]);
                }
            }
        }
    }

    [Fact]
    [Trait("Category", "Pathfinding")]
    public void FindPath_ShouldReturnSamePath_WhenCalledWithSameInstance()
    {
        var world = TestWorldFactory.CreateWorld();
        var start = new GridPosition(1, 1);
        var target = new GridPosition(8, 8);

        var path1 = PathFinder.FindPath(world, start, target).ToList();
        var path2 = PathFinder.FindPath(world, start, target).ToList();

        Assert.Equal(path1.Count, path2.Count);
        for (int i = 0; i < path1.Count; i++)
        {
            Assert.Equal(path1[i], path2[i]);
        }
    }
}
