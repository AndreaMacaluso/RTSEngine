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

public sealed class TerrainPatchDefinition
{
    public required string Type { get; init; }
    public int CountPerSide { get; init; }
    public int MinRadius { get; init; } = 1;
    public int MaxRadius { get; init; } = 2;
}

public sealed class ResourceClusterDefinition
{
    public required string Type { get; init; }
    public int ClustersPerPlayer { get; init; }
    public int NodesPerCluster { get; init; }
    public int MinDistanceFromSpawn { get; init; }
    public int MaxDistanceFromSpawn { get; init; }
    public int ClusterRadius { get; init; } = 2;
}
