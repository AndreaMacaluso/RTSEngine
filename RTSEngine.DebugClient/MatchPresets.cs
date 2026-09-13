using RTSEngine.Core.Settings;

namespace RTSEngine.DebugClient;

public static class MatchPresets
{
    public static GameSettings RandomMapConquest => new()
    {
        MapPath = "Data/Maps/map_00.json",
        Mode = GameMode.RandomMap,
        Victory = VictoryType.Conquest,
        NumPlayers = 2
    };

    public static GameSettings WaveBattle => new()
    {
        Mode = GameMode.Wave,
        Victory = VictoryType.Score,
        NumPlayers = 2,
        Width = 40,
        Height = 40
    };
}
