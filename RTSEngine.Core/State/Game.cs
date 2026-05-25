using RTSEngine.Core.Map;
using RTSEngine.Core.Map.Runtime;
// using RTSEngine.Core.Entities;

namespace RTSEngine.Core.State;

public class GameWorld
{
    public TileMap Map { get; }

     //  public List<Entity> Entities { get; }

    public int CurrentTick { get; private set; }

    public GameWorld(TileMap map)
    {
        Map = map;
        // Entities = new List<Entity>();
        CurrentTick = 0;
    }

    public void AdvanceTick()
    {
        CurrentTick++;
    }
}