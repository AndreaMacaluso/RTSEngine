namespace RTSEngine.Core.Map.Loading;

using RTSEngine.Core.Map.Loading;
using RTSEngine.Core.State;
using RTSEngine.Core.Simulation;
using RTSEngine.Core.Map.Runtime;
public class WorldBuilder
{
    public static GameWorld Build(Definitions.MapData data)
    {
        var builder = new TileMapBuilder();

        var tileMap = builder.Build(data);
       
        var resources = data.Resources
            .Select(ResourceFactory.Create)
            .ToList();
        return new GameWorld(
            tileMap,
            resources,
            data.Spawns);
    }
        
}