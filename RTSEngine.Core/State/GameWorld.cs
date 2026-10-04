using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Players;
using RTSEngine.Core.Entities.Resources;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Map.Definitions;
using RTSEngine.Core.Map.Visibility;
using RTSEngine.Core.Settings;

namespace RTSEngine.Core.State;

public class GameWorld
{
    public TileMap Map { get; }
    public int CurrentTick { get; private set; }
    public WorldState State { get; private set; } = WorldState.Running;

    private readonly List<SpawnPointDefinition> _spawns = [];
    private readonly List<Player> _players = [];

    public RuntimeEntities Entities { get; }
    public ProjectileState Projectiles { get; } = new();
    public IReadOnlyList<SpawnPointDefinition> Spawns => _spawns;
    public IReadOnlyList<Player> Players => _players;

    /// <summary>
    /// Per-player visibility state; no grid at all in MapVisibility.AllVisible
    /// (FogOfWar.IsEnabled).
    ///
    /// The mode is one-shot wiring: GameWorld does not hold GameSettings,
    /// so the call site that has them passes it in.
    /// Changing it later means rebuilding the world, which is what lockstep
    /// wants anyway.
    /// </summary>
    public FogOfWar Fog { get; }

    /// <summary>
    /// The world as this player sees it: that player's grid plus the entities
    /// that belong to their view. Built on demand, never on the hot path.
    /// </summary>
    public PlayerView ViewFor(int playerId) => new(this, playerId);

    public GameWorld(
        TileMap map,
        List<ResourceNode>? resources = null,
        List<SpawnPointDefinition>? spawns = null,
        MapVisibility visibility = MapVisibility.AllVisible)
    {
        Map = map;
        Fog = new FogOfWar(map.Width, map.Height, visibility);
        Entities = new RuntimeEntities(_players);

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

    public void AddPlayer(Player player)
    {
        _players.Add(player);
        Fog.OnPlayerAdded(player.Id);
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
