using RTSEngine.Core.Diagnostics;
using RTSEngine.Core.Entities.Runtime;

namespace RTSEngine.Core.Triggers;

public sealed class SendChatEffect : ITriggerEffect
{
    private readonly int _playerId;
    private readonly string _message;

    public SendChatEffect(int playerId, string message)
    {
        _playerId = playerId;
        _message = message;
    }

    public void Execute(RuntimeContext context)
    {
        DebugSession.Log.Info($"[Player {_playerId}] {_message}");
    }
}
