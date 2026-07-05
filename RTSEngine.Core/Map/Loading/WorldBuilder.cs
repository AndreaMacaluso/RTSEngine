namespace RTSEngine.Core.Map.Loading;

using RTSEngine.Core.State;
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