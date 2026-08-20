using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Entities;
using RTSEngine.Core.Players;
using RTSEngine.Core.Entities.Resources;
using RTSEngine.Core.Map.Definitions;
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

    private readonly List<Entity> _entities = [];
    private readonly List<ResourceNode> _resources = [];
    private readonly List<SpawnPointDefinition> _spawns = [];
    private readonly List<Player> _players = [];
    private readonly Queue<ICommand> _pendingCommands = [];

    public IReadOnlyList<Entity> Entities => _entities;
    public IReadOnlyList<ResourceNode> Resources => _resources;
    public IReadOnlyList<SpawnPointDefinition> Spawns => _spawns;
    public IReadOnlyList<Player> Players => _players;
    public IReadOnlyCollection<ICommand> PendingCommands => _pendingCommands;

    public IEnumerable<Building> Buildings => _entities.OfType<Building>();

    private int _nextEntityId = 1;

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
        _spawns.AddRange(spawns ?? []);
        CurrentTick = 0;
    }

    public void AdvanceTick()
    {
        CurrentTick++;
        DebugSession.Log.Info($"CurrentTick: {CurrentTick}");
    }

    public Entity? GetEntityAt(int x, int y)
    {
        return _entities.FirstOrDefault(
            e => e.Position.X == x
            && e.Position.Y == y);
    }

    public int GenerateEntityId()
    {
        return _nextEntityId++;
    }

    public void AddEntity(Entity entity)
    {
        entity.Id = GenerateEntityId();
        _entities.Add(entity);
    }

    public void AddCommand(ICommand command)
    {
        _pendingCommands.Enqueue(command);
    }

    public ICommand? DequeueCommand()
    {
        return _pendingCommands.Count > 0
            ? _pendingCommands.Dequeue()
            : null;
    }

    public void AddPlayer(Player player)
    {
        _players.Add(player);
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
        _resources.Add(resource);
    }

    public ResourceNode? GetResourceById(int id)
    {
        return _resources.FirstOrDefault(r => r.Id == id);
    }

    public Player? GetPlayerById(int id)
    {
        return _players.FirstOrDefault(p => p.Id == id);
    }

    public Entity? GetEntityById(int id)
    {
        return _entities.FirstOrDefault(e => e.Id == id);
    }

    public Building? GetBuildingById(int id)
    {
        return GetEntityById(id) as Building;
    }

    public Unit? GetUnitById(int id)
    {
        return GetEntityById(id) as Unit;
    }

    public void RemoveEntity(Entity entity)
    {
        _entities.Remove(entity);
    }

    public void RemoveResource(ResourceNode resource)
    {
        _resources.Remove(resource);
    }
}
