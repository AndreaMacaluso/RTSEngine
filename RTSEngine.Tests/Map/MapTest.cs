namespace RTSEngine.Tests.Map;
using RTSEngine.Core.Map;
using Xunit;

public class MapTest
{
    [Fact]
    public void TileMap_HasCorrectSize()
    {
        var map = new TileMap(40, 40);

        Assert.Equal(40, map.Width);
        Assert.Equal(40, map.Height);
    }
}