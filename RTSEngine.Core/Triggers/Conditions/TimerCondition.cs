using RTSEngine.Core.Diagnostics;
using RTSEngine.Core.Entities.Runtime;

namespace RTSEngine.Core.Triggers;

public sealed class TimerCondition : ITriggerCondition
{
    private readonly int _interval;
    private readonly int _startTick;

    public TimerCondition(int interval, int startTick = 0)
    {
        _interval = interval;
        _startTick = startTick;
    }

    public bool Evaluate(RuntimeContext context)
    {
        var currentTick = context.World.CurrentTick;

        if (currentTick < _startTick)
            return false;

        var result = _interval > 0 && currentTick % _interval == 0;
        DebugSession.Log.Debug($"[TimerCondition] Interval: {_interval}, StartTick: {_startTick}, Tick: {currentTick}, Result: {result}");
        return result;
    }
}
