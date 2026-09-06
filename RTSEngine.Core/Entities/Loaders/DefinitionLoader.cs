using System.Text.Json;
using System.Text.Json.Serialization;

namespace RTSEngine.Core.Entities.Loaders;

public static class DefinitionLoader<T>
{
    public static List<T> Load(string path)
    {
        var json = File.ReadAllText(path);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };

        return JsonSerializer.Deserialize<List<T>>(json, options)
            ?? [];
    }
}
