using System.Text.Json;
namespace RTSEngine.Core.Map.Loading;

public class JsonMapLoader
{
    public Definitions.MapData Load(string path)
    {
        var json = File.ReadAllText(path);

        return JsonSerializer.Deserialize<Definitions.MapData>(json)!;
    }
}