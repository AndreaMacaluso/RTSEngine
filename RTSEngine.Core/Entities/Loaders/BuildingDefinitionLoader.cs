using System.Text.Json;
using RTSEngine.Core.Entities.Definitions;

namespace RTSEngine.Core.Entities.Loader;

public class BuildingDefinitionLoader
{
    public List<BuildingDefinition> Load(
        string path)
    {
        var json = File.ReadAllText(path);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        return JsonSerializer.Deserialize<List<BuildingDefinition>>(json, options)
            ?? [];
    }
}