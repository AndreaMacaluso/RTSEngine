using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Systems;

namespace RTSEngine.Tests.Systems;

public class PathSystemTests
{
    [Fact]
    [Trait("Category", "Movement")]
    public void GeneratePath_ShouldReachTarget()
    {
        var path = PathSystem.GeneratePath(
            new GridPosition(1,1),
            new GridPosition(5,5));

        Assert.NotEmpty(path);

        var last = path.Last();

        Assert.Equal(5, last.X);
        Assert.Equal(5, last.Y);
    }

    [Fact]
    [Trait("Category", "Movement")]
    public void GeneratePath_ShouldCreateExpectedStepCount()
    {
        var path = PathSystem.GeneratePath(
            new GridPosition(1,1),
            new GridPosition(5,5));

        Assert.Equal(4, path.Count);
    }
}