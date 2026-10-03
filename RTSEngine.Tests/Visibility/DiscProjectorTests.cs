using RTSEngine.Core.Map.Visibility;
using Xunit;

namespace RTSEngine.Tests.Visibility;

[Trait("Category", "Visibility")]
public class DiscProjectorTests
{
    [Fact]
    public void RangeAboveTheCap_EqualsTheCap()
    {
        var capped = DiscProjector.Instance.Offsets(64);
        var beyond = DiscProjector.Instance.Offsets(65);

        Assert.Equal(capped, beyond);
    }

    [Fact]
    public void RangeZero_SeesOnlyItsOwnTile()
        => Assert.Equal((0, 0), Assert.Single(DiscProjector.Instance.Offsets(0)));

    [Fact]
    public void NegativeRange_SeesOnlyItsOwnTile()
        => Assert.Equal((0, 0), Assert.Single(DiscProjector.Instance.Offsets(-5)));

    [Fact]
    public void Range1_IsACross_FiveTilesNotNine()
    {
        var offsets = DiscProjector.Instance.Offsets(1);

        // a square of side 3 has 9 tiles; the corners are outside the disc
        Assert.Equal(5, offsets.Count);
        Assert.Contains((0, 0), offsets);
        Assert.Contains((1, 0), offsets);
        Assert.Contains((-1, 0), offsets);
        Assert.Contains((0, 1), offsets);
        Assert.Contains((0, -1), offsets);
        Assert.DoesNotContain((1, 1), offsets);
        Assert.DoesNotContain((-1, -1), offsets);
    }

    [Fact]
    public void SameRange_ReturnsTheSameArrayInstance()
        => Assert.Same(
            DiscProjector.Instance.Offsets(4),
            DiscProjector.Instance.Offsets(4));
}
