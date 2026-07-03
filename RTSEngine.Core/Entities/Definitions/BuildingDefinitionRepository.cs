namespace RTSEngine.Core.Entities.Definitions;

public sealed class BuildingDefinitionRepository
{
    private readonly Dictionary<string, BuildingDefinition> _definitions;

    public BuildingDefinitionRepository(
        IEnumerable<BuildingDefinition> definitions)
    {
        _definitions = definitions.ToDictionary(
            d => d.Id,
            d => d);
    }

    public BuildingDefinition Get(string id)
    {
        return _definitions[id];
    }

    public bool Exists(string id)
    {
        return _definitions.ContainsKey(id);
    }
}