using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Players;
using RTSEngine.Core.Entities.Resources;
using RTSEngine.Core.Map.Definitions;
using RTSEngine.Core.Commands;

namespace RTSEngine.Core.State;

public class GameWorld
{
    public TileMap Map { get; }
    public int CurrentTick { get; private set; }
    public WorldState State { get; private set; } = WorldState.Running;

    private readonly List<SpawnPointDefinition> _spawns = [];
    private readonly List<Player> _players = [];
    private readonly Queue<ICommand> _pendingCommands = [];

    public Entities Entities { get; }
    public IReadOnlyList<SpawnPointDefinition> Spawns => _spawns;
    public IReadOnlyList<Player> Players => _players;
    public IReadOnlyCollection<ICommand> PendingCommands => _pendingCommands;
    
    public GameWorld(
        TileMap map,
        List<ResourceNode>? resources = null,
        List<SpawnPointDefinition>? spawns = null)
    {
        Map = map;
        Entities = new Entities(_players);

        foreach (var resource in resources ?? [])
        {
            Entities.Add(resource);
        }
        _spawns.AddRange(spawns ?? []);
        CurrentTick = 0;
    }

    public void AdvanceTick()
    {
        CurrentTick++;
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

    public Player? GetPlayerById(int id)
    {
        return _players.FirstOrDefault(p => p.Id == id);
    }
}
