namespace RTSEngine.Core.Map.Definitions;

public sealed class TerrainPatchDefinition
{
    public required string Type { get; init; }
    public int CountPerSide { get; init; }
    public int MinRadius { get; init; } = 1;
    public int MaxRadius { get; init; } = 2;
}
