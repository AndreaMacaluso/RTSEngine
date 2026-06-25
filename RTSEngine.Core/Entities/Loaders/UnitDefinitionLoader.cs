using System.Text.Json;

namespace RTSEngine.Core.Entities.Loader;

public class UnitDefinitionLoader
{
    public List<UnitDefinition> Load(
        string path)
    {
        var json = File.ReadAllText(path);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };


        return JsonSerializer.Deserialize<List<UnitDefinition>>(json, options)
            ?? [];
    }
}