using System.Text.Json;
using System.Text.Json.Serialization;
using RTSEngine.Core.Diagnostics;

namespace RTSEngine.Core.Triggers;

public static class MissionLoader
{
    public static MissionDefinition Load(string path)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };

        try
        {
            if (!File.Exists(path))
            {
                DebugSession.Log.Error($"Mission file not found: {path}");
                return new MissionDefinition();
            }

            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<MissionDefinition>(json, options) ?? new MissionDefinition();
        }
        catch (Exception ex)
        {
            DebugSession.Log.Error($"Failed to load mission {path}: {ex.Message}");
            return new MissionDefinition();
        }
    }
}
