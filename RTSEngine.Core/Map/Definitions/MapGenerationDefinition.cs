namespace RTSEngine.Core.Map.Definitions;

public sealed class MapGenerationDefinition
{
    public int Seed { get; init; }
    public int Width { get; init; } = 40;
    public int Height { get; init; } = 40;
    public int SpawnInset { get; init; } = 7;
    public int BaseClearRadius { get; init; } = 5;
    public int ResourceSeparation { get; init; } = 1;
    public List<TerrainPatchDefinition> TerrainPatches { get; init; } = [];
    public List<ResourceClusterDefinition> ResourceClusters { get; init; } = [];
}
