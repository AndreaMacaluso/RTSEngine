namespace RTSEngine.Core.Entities.Definitions;

public class UnitDefinitionRepository
{
    private readonly Dictionary<string, UnitDefinition> _definitions;

    public UnitDefinitionRepository(
        IEnumerable<UnitDefinition> definitions)
    {
        _definitions = definitions.ToDictionary(
            d => d.Id,
            d => d);
    }

    public UnitDefinition Get(
        string id)
    {
        return _definitions[id];
    }

    public bool Exists(
        string id)
    {
        return _definitions.ContainsKey(id);
    }
}