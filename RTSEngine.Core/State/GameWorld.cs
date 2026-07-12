using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Entities;
using RTSEngine.Core.Players;
using RTSEngine.Core.Entities.Resources;
using RTSEngine.Core.Map.Definitions;
using RTSEngine.Core.Map.Rules;
using RTSEngine.Core.Commands;
using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Diagnostics;
using RTSEngine.Core.Entities.Buildings;
namespace RTSEngine.Core.State;

public class GameWorld
{
    public TileMap Map { get; }
    public int CurrentTick { get; private set; }
    public WorldState State { get; private set; } = WorldState.Running;
    public List<Entity> Entities { get; } = [];
    public List<ResourceNode> Resources { get; } = [];
    public List<SpawnPointDefinition> Spawns { get; } = [];
    private int _nextEntityId = 1;
    public List<Player> Players { get; } = [];

    public Queue<ICommand> PendingCommands { get; } = [];
    public GameWorld(
        TileMap map,
        List<ResourceNode>? resources = null,
        List<SpawnPointDefinition>? spawns = null)
    {
        Map = map;

        foreach (var resource in resources ?? [])
        {
            AddResource(resource);
        }
        Spawns = spawns ?? [];
        Entities = [];
        CurrentTick = 0;
    }
    public void AdvanceTick()
    {   
        CurrentTick++;
        DebugSession.Log.Info($"CurrentTick: {CurrentTick}");
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
    public void AddCommand(ICommand command)
    {
        PendingCommands.Enqueue(command);
    }

    public void AddPlayer(Player player)
    {
        Players.Add(player);
    }
    public void Pause()
    {
        State = WorldState.Paused;
    }

    public void Resume()
    {
        State = WorldState.Running;
    }

    public void Finish()
    {
        State = WorldState.Finished;
    }

    public void AddResource(ResourceNode resource)
    {
        resource.Id = GenerateEntityId();

        Resources.Add(resource);
    }
    
    public ResourceNode? GetResourceById(int id)
    {
        return Resources.FirstOrDefault(r => r.Id == id);
    }
    public Player? GetPlayerById(int id)
    {
        return Players.FirstOrDefault(p => p.Id == id);
    }
    public Entity? GetEntityById(int id)
    {
        return Entities.FirstOrDefault(e => e.Id == id);
    }
    public Building? GetBuildingById(int id)
    {
        return GetEntityById(id) as Building;
    }
     public Unit? GetUnitById(int id)
    {
        return GetEntityById(id) as Unit;
    }

    public IEnumerable<Building> GetBuildings()
    {
        return Entities.OfType<Building>();
    }

}