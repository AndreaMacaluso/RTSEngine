using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Entities;
using RTSEngine.Core.Entities.Resources;
using RTSEngine.Core.Map.Definitions;

namespace RTSEngine.Core.State;

public class GameWorld
{
    public TileMap Map { get; }
    public int CurrentTick { get; private set; }
    public List<Entity> Entities { get; } = [];
    public List<ResourceNode> Resources { get; } = [];
    public List<SpawnPointDefinition> Spawns { get; } = [];


     public GameWorld(
        TileMap map,
        List<ResourceNode> resources,
        List<SpawnPointDefinition> spawns)
    {
        Map = map;
        Resources = resources;
        Spawns = spawns;
        Entities = [];
        CurrentTick = 0;
    }
    public void AdvanceTick()
    {
        CurrentTick++;
    }
}