using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Entities;
using RTSEngine.Core.Entities.Resources;
using RTSEngine.Core.Map.Definitions;
using RTSEngine.Core.Map.Rules;

namespace RTSEngine.Core.State;

public class GameWorld
{
    public TileMap Map { get; }
    public int CurrentTick { get; private set; }
    public List<Entity> Entities { get; } = [];
    public List<ResourceNode> Resources { get; } = [];
    public List<SpawnPointDefinition> Spawns { get; } = [];
    private int _nextEntityId = 1;
    public GameWorld(
        TileMap map,
        List<ResourceNode>? resources = null,
        List<SpawnPointDefinition>? spawns = null)
    {
        Map = map;
        Resources = resources ?? [];
        Spawns = spawns ?? [];
        Entities = [];
        CurrentTick = 0;
    }
    public void AdvanceTick()
    {
        CurrentTick++;
    }

    public Entity? GetEntityAt(int x, int y)
    {
        return Entities.FirstOrDefault(
            e => e.Position.X == x
            && e.Position.Y == y);
    }
    public bool IsTileOccupied(int x, int y)
    {
        return GetEntityAt(x, y) != null;
    }
    public bool IsInsideBounds(int x, int y)
    {
        return x >= 0
            && y >= 0
            && x < Map.Width
            && y < Map.Height;
    }

    public bool IsTileBlocked(int x, int y)
    {
        if (!IsInsideBounds(x, y))
        {
            return true;
        }

        var tile = Map.GetTile(x, y);

        if (!TileRules.IsWalkable(tile))
        {
            return true;
        }

        var entity = GetEntityAt(x, y);

        return entity?.IsBlocking ?? false;
    }

    public int GenerateEntityId()
    {
        return _nextEntityId++;
    }

    public void AddEntity(Entity entity)
    {
        entity.Id = GenerateEntityId();

        Entities.Add(entity);
    }
}