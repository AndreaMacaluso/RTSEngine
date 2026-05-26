namespace RTSEngine.Core.Map.Definitions;

public class MapData
{
    public string Name { get; set;} = string.Empty;
    public int Width { get; set; }

    public int Height { get; set; }

    public List<string> Rows { get; set; } = new();

    public required List<ResourceDefinition> Resources { get; init; }

    public required List<SpawnPointDefinition> Spawns { get; init; }
}
