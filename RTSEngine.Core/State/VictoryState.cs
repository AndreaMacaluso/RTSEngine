using RTSEngine.Core.Players;
using RTSEngine.Core.Settings;

namespace RTSEngine.Core.State;

public sealed class VictoryState
{
    public int? WinnerPlayerId { get; private set; }

    public void Check(IReadOnlyList<Player> players, VictoryCondition condition)
    {
        if (WinnerPlayerId.HasValue)
        {
            return;
        }

        switch (condition.Type)
        {
            case VictoryConditionType.Conquest:
                CheckConquest(players);
                break;
            case VictoryConditionType.ScoreLimit:
                CheckScoreLimit(players, condition.ScoreTarget);
                break;
        }
    }

    private void CheckConquest(IReadOnlyList<Player> players)
    {
        Player? lastWithBuildings = null;
        int count = 0;

        foreach (var player in players)
        {
            if (player.BuildingIds.Count > 0)
            {
                count++;
                lastWithBuildings = player;
            }
        }

        if (count == 1 && lastWithBuildings != null)
        {
            WinnerPlayerId = lastWithBuildings.Id;
        }
    }

    private void CheckScoreLimit(IReadOnlyList<Player> players, int scoreTarget)
    {
        foreach (var player in players)
        {
            if (player.Score >= scoreTarget)
            {
                WinnerPlayerId = player.Id;
                return;
            }
        }
    }
}
