using RTSEngine.Core.Settings;
using RTSEngine.Core.State.Snapshots;
using Xunit;

namespace RTSEngine.Tests.SaveLoad;

[Trait("Category", "SaveLoad")]
public class VisibilitySettingsTests
{
    [Fact]
    public void AllModes_SurviveTheRoundTrip()
    {
        foreach (var mode in Enum.GetValues<MapVisibility>())
        {
            var dto = SettingsSnapshot.From(new GameSettings { Visibility = mode });

            Assert.Equal(mode, dto.ToVisibility());
        }
    }

    [Fact]
    public void InvalidString_TheFogStaysOn()
        => Assert.Equal(MapVisibility.Normal, new SettingsSnapshot { Visibility = "Banana" }.ToVisibility());
}
