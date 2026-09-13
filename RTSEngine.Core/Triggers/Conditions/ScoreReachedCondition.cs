using RTSEngine.Core.Entities.Runtime;

namespace RTSEngine.Core.Triggers;

public sealed class ScoreReachedCondition : ITriggerCondition
{
    private readonly int _playerId;
    private readonly int _scoreTarget;

    public ScoreReachedCondition(int playerId, int scoreTarget)
    {
        _playerId = playerId;
        _scoreTarget = scoreTarget;
    }

    public bool Evaluate(RuntimeContext context)
    {
        var world = context.World;
        var player = world.GetPlayerById(_playerId);

        if (player == null)
        {
            return false;
        }

        return player.Score >= _scoreTarget;
    }
}
