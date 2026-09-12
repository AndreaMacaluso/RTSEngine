using RTSEngine.Core.Commands;
using RTSEngine.Core.Diagnostics;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Helpers;
using RTSEngine.Core.Map.Runtime;

namespace RTSEngine.Core.Triggers;

public sealed class AttackMoveEffect : ITriggerEffect
{
    private readonly int _playerId;
    private readonly int _targetX;
    private readonly int _targetY;

    public AttackMoveEffect(int playerId, int targetX, int targetY)
    {
        _playerId = playerId;
        _targetX = targetX;
        _targetY = targetY;
    }

    public void Execute(RuntimeContext context)
    {
        DebugSession.Log.Debug($"[AttackMoveEffect] Player: {_playerId}, Target: ({_targetX},{_targetY})");

        var world = context.World;
        var player = world.GetPlayerById(_playerId);
        if (player == null)
        {
            DebugSession.Log.Debug($"[AttackMoveEffect] Player {_playerId} not found");
            return;
        }

        var militaryUnits = UnitQueries.FindIdleMilitary(world, player);
        var unitIds = militaryUnits.Select(u => u.Id).ToList();

        if (unitIds.Count == 0)
        {
            DebugSession.Log.Debug($"[AttackMoveEffect] Player {_playerId} has no idle military units");
            return;
        }

        DebugSession.Log.Debug($"[AttackMoveEffect] Sending {unitIds.Count} military units to attack move");

        var command = new AttackCommand
        {
            UnitIds = unitIds,
            Mode = AttackMode.AttackMove,
            TargetPosition = new GridPosition(_targetX, _targetY)
        };

        context.CommandQueue.Enqueue(command);
    }
}
