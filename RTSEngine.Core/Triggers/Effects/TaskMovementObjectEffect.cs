using RTSEngine.Core.Commands;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Map.Runtime;

namespace RTSEngine.Core.Triggers;

public sealed class TaskMovementObjectEffect : ITriggerEffect
{
    private readonly int _playerId;
    private readonly int _targetX;
    private readonly int _targetY;

    public TaskMovementObjectEffect(int playerId, int targetX, int targetY)
    {
        _playerId = playerId;
        _targetX = targetX;
        _targetY = targetY;
    }

    public void Execute(RuntimeContext context)
    {
        var world = context.World;
        var player = world.GetPlayerById(_playerId);
        if (player == null) return;

        var unitIds = player.UnitIds.ToList();
        if (unitIds.Count == 0) return;

        var command = new MoveCommand
        {
            UnitIds = unitIds,
            Target = new GridPosition(_targetX, _targetY)
        };

        context.CommandQueue.Enqueue(command);
    }
}
