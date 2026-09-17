using RTSEngine.Core.Helpers;

namespace RTSEngine.Tests.Helpers;

public class SequentialIdGeneratorTests
{
    [Fact]
    [Trait("Category", "Helpers")]
    public void Next_ShouldReturnSequentialIds()
    {
        var generator = new SequentialIdGenerator();

        Assert.Equal(1, generator.Next());
        Assert.Equal(2, generator.Next());
        Assert.Equal(3, generator.Next());
    }

    [Fact]
    [Trait("Category", "Helpers")]
    public void Next_ShouldStartAtCustomValue()
    {
        var generator = new SequentialIdGenerator(startId: 100);

        Assert.Equal(100, generator.Next());
        Assert.Equal(101, generator.Next());
    }

    [Fact]
    [Trait("Category", "Helpers")]
    public void Reset_ShouldResetToSeed()
    {
        var generator = new SequentialIdGenerator();

        generator.Next();
        generator.Next();
        generator.Reset(50);

        Assert.Equal(50, generator.Next());
    }
}
