using RTSEngine.Core.Diagnostics;
using RTSEngine.Core.Entities.Runtime;

namespace RTSEngine.Core.Triggers;

public sealed class OwnObjectsCondition : ITriggerCondition
{
    private readonly int _playerId;
    private readonly string _objectTypeId;
    private readonly int _quantity;

    public OwnObjectsCondition(int playerId, string objectTypeId, int quantity)
    {
        _playerId = playerId;
        _objectTypeId = objectTypeId;
        _quantity = quantity;
    }

    public bool Evaluate(RuntimeContext context)
    {
        var world = context.World;
        var player = world.GetPlayerById(_playerId);
        if (player == null)
        {
            DebugSession.Log.Warning($"Player {_playerId} not found in OwnObjectsCondition");
            return false;
        }

        var count = 0;
        foreach (var unitId in player.UnitIds)
        {
            var unit = world.Entities.GetUnitById(unitId);
            if (unit != null && unit.Definition.Id == _objectTypeId)
                count++;
        }

        return count >= _quantity;
    }
}
