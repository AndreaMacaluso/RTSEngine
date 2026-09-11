using System.Text.Json;
using RTSEngine.Core.Diagnostics;

namespace RTSEngine.Core.Triggers;

public static class MissionLoader
{
    public static MissionDefinition Load(string missionId)
    {
        try
        {
            var path = $"missions/{missionId}.json";
            if (!File.Exists(path))
            {
                DebugSession.Log.Error($"Mission file not found: {path}");
                return new MissionDefinition();
            }

            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<MissionDefinition>(json) ?? new MissionDefinition();
        }
        catch (Exception ex)
        {
            DebugSession.Log.Error($"Failed to load mission {missionId}: {ex.Message}");
            return new MissionDefinition();
        }
    }
}
