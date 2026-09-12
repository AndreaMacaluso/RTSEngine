using RTSEngine.Core.Diagnostics;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Helpers;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.State;

namespace RTSEngine.Core.Triggers;

public sealed class CreateObjectEffect : ITriggerEffect
{
    private readonly int _playerId;
    private readonly string _objectTypeId;
    private readonly int _locationX;
    private readonly int _locationY;

    public CreateObjectEffect(int playerId, string objectTypeId, int locationX, int locationY)
    {
        _playerId = playerId;
        _objectTypeId = objectTypeId;
        _locationX = locationX;
        _locationY = locationY;
    }

    public void Execute(RuntimeContext context)
    {
        DebugSession.Log.Debug($"[CreateObjectEffect] Player: {_playerId}, Type: {_objectTypeId}, Pos: ({_locationX},{_locationY})");

        var world = context.World;
        var player = world.GetPlayerById(_playerId);
        if (player == null)
        {
            DebugSession.Log.Debug($"[CreateObjectEffect] Player {_playerId} not found");
            return;
        }

        if (!context.UnitRepository.Exists(_objectTypeId))
        {
            DebugSession.Log.Debug($"[CreateObjectEffect] Unit type '{_objectTypeId}' not found");
            return;
        }

        var definition = context.UnitRepository.Get(_objectTypeId);
        var position = new GridPosition(_locationX, _locationY);

        if (WorldQueries.IsTileBlocked(world, position.X, position.Y))
        {
            var adjacent = WorldQueries.FindAdjacentWalkableTile(world, position);
            if (adjacent.HasValue)
                position = adjacent.Value;
        }

        var unit = new Unit(_playerId, position, definition);
        world.Entities.Add(unit, player);

        DebugSession.Log.Debug($"[CreateObjectEffect] Unit created: {unit.Id} at ({position.X},{position.Y})");
    }
}
