using RTSEngine.Core.Entities.Runtime;

namespace RTSEngine.Core.Triggers;

public sealed class AssignVictoryEffect : ITriggerEffect
{
    private readonly List<int> _playerIds;

    public AssignVictoryEffect(IEnumerable<int> playerIds)
    {
        _playerIds = playerIds.ToList();
    }

    public void Execute(RuntimeContext context)
    {
        foreach (var playerId in _playerIds)
        {
            var player = context.World.GetPlayerById(playerId);
            if (player != null)
            {
                player.IsWinner = true;
            }
        }
    }
}
