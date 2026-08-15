namespace RTSEngine.Core.Map.Loading;

using RTSEngine.Core.Map.Generation;
using RTSEngine.Core.State;
public class WorldBuilder
{
    public static GameWorld Build(Definitions.MapData data)
    {
        if (data.Generation is not null)
        {
            data = SymmetricMapGenerator.Generate(data.Name, data.Generation);
        }

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
