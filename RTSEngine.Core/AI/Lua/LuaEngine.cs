using MoonSharp.Interpreter;
using RTSEngine.Core.Diagnostics;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Players;
using RTSEngine.Core.State;

namespace RTSEngine.Core.AI.Lua;

public class LuaEngine
{
    private readonly Dictionary<int, LuaAiScript> _scripts = new();
    private readonly string _scriptsPath;

    public LuaEngine(string scriptsPath)
    {
        _scriptsPath = scriptsPath;
        SetupLua();
    }

    private void SetupLua()
    {
        UserData.RegisterType<AiApi>();
    }

    public LuaAiScript? LoadScript(Player player, string scriptName)
    {
        var scriptPath = Path.Combine(_scriptsPath, $"{scriptName}.lua");

        if (!File.Exists(scriptPath))
        {
            DebugSession.Log.Warning(
                "LuaEngine.LoadScript: script not found",
                [("Path", scriptPath)]);
            return null;
        }

        var script = new LuaAiScript(player, scriptPath);

        if (_scripts.ContainsKey(player.Id))
        {
            _scripts[player.Id].Dispose();
        }

        _scripts[player.Id] = script;

        DebugSession.Log.Info(
            "LuaEngine.LoadScript",
            [("PlayerId", player.Id), ("Script", scriptName)]);

        return script;
    }

    public void Update(RuntimeContext context)
    {
        foreach (var player in context.World.Players)
        {
            if (player.Controller != PlayerControllerType.AI)
                continue;

            if (!_scripts.TryGetValue(player.Id, out var script))
                continue;

            script.Update(context);
        }
    }

    public void Dispose()
    {
        foreach (var script in _scripts.Values)
        {
            script.Dispose();
        }
        _scripts.Clear();
    }
}
