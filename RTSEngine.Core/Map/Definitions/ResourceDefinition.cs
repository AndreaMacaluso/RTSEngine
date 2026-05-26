namespace RTSEngine.Core.Map.Definitions;
public sealed class ResourceDefinition
{
    public required string Type { get; init; }

    public required int X { get; init; }

    public required int Y { get; init; }
}