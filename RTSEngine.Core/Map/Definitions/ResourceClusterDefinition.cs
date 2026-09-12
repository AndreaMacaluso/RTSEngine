namespace RTSEngine.Core.Map.Definitions;

public sealed class ResourceClusterDefinition
{
    public required string Type { get; init; }
    public int ClustersPerPlayer { get; init; }
    public int NodesPerCluster { get; init; }
    public int MinDistanceFromSpawn { get; init; }
    public int MaxDistanceFromSpawn { get; init; }
    public int ClusterRadius { get; init; } = 2;
}
