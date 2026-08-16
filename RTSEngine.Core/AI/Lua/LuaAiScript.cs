using MoonSharp.Interpreter;
using RTSEngine.Core.Diagnostics;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Players;
using RTSEngine.Core.State;

namespace RTSEngine.Core.AI.Lua;

public class LuaAiScript : IDisposable
{
    private readonly Player _player;
    private readonly Script _script;
    private AiApi _api;
    private bool _disposed;

    public Player Player => _player;
    public Table? Globals => _script.Globals;

    public LuaAiScript(Player player, string scriptPath)
    {
        _player = player;
        _script = new Script();

        _api = new AiApi(null!, player);

        _script.Globals["ai"] = _api;

        try
        {
            _script.DoFile(scriptPath);
        }
        catch (SyntaxErrorException ex)
        {
            DebugSession.Log.Error(
                "LuaAiScript: syntax error",
                [("PlayerId", player.Id), ("Error", ex.Message)]);
            throw;
        }
        catch (ScriptRuntimeException ex)
        {
            DebugSession.Log.Error(
                "LuaAiScript: runtime error loading script",
                [("PlayerId", player.Id), ("Error", ex.Message)]);
            throw;
        }
    }

    public void Update(RuntimeContext context)
    {
        if (_disposed) return;

        _api = new AiApi(context, _player);
        _script.Globals["ai"] = _api;

        try
        {
            var onTick = _script.Globals.Get("onTick");

            if (onTick != null && onTick.Type == DataType.Function)
            {
                _script.Call(onTick);
            }
        }
        catch (ScriptRuntimeException ex)
        {
            DebugSession.Log.Error(
                "LuaAiScript: runtime error in onTick",
                [("PlayerId", _player.Id), ("Error", ex.Message)]);
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
    }
}
