namespace RTSEngine.Core.Map.Definitions;

public sealed class SpawnPointDefinition
{
    public required int PlayerId { get; init; }

    public required int X { get; init; }

    public required int Y { get; init; }
}