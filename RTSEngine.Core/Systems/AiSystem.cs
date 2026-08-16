using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.AI;
using RTSEngine.Core.AI.Lua;
using RTSEngine.Core.Players;

namespace RTSEngine.Core.Systems;


public static class AISystem
{
    private static LuaEngine? _luaEngine;

    public static void Initialize(string scriptsPath)
    {
        _luaEngine = new LuaEngine(scriptsPath);
    }

    public static void LoadLuaScript(
        RuntimeContext context,
        Player player,
        string scriptName)
    {
        if (_luaEngine == null) return;
        _luaEngine.LoadScript(player, scriptName);
    }

    public static void Update(RuntimeContext context)
    {
        if (_luaEngine != null)
        {
            _luaEngine.Update(context);
        }

        foreach (var player in context.World.Players)
        {
            if (player.Controller != PlayerControllerType.AI)
            {
                continue;
            }
            if (context.World.CurrentTick - player.AI.LastDecisionTick < 10)
            {
                  continue;
            }
            player.AI.LastDecisionTick = context.World.CurrentTick;
            BasicAI.Update(context, player);
        }
    }
}