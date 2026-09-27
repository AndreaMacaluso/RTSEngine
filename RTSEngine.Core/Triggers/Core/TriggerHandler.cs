using RTSEngine.Core.Diagnostics;
using RTSEngine.Core.Entities.Runtime;

namespace RTSEngine.Core.Triggers;

public sealed class TriggerHandler
{
    private readonly Dictionary<string, ITrigger> _triggers = new();
    private readonly List<ITrigger> _sortedTriggers = new();
    private bool _dirty = true;

    public int TriggerCount => _triggers.Count;

    public void RegisterTrigger(ITrigger trigger)
    {
        _triggers[trigger.Id] = trigger;
        _dirty = true;
    }

    public void Update(RuntimeContext context)
    {
        if (_dirty)
        {
            _sortedTriggers.Clear();
            _sortedTriggers.AddRange(_triggers.Values);
            _sortedTriggers.Sort((a, b) => a.Id.CompareTo(b.Id));
            _dirty = false;
        }

        foreach (var trigger in _sortedTriggers)
        {
            try
            {
                if (!trigger.Enabled) continue;

                if (!trigger.Looping && trigger.HasExecuted) continue;

                DebugSession.Log.Debug($"[Trigger] Evaluating: {trigger.Id} (Tick: {context.World.CurrentTick})");

                var allConditionsMet = trigger.Conditions.Count == 0 || AllConditionsMet(trigger, context);
                if (!allConditionsMet)
                {
                    DebugSession.Log.Debug($"[Trigger] {trigger.Id}: conditions NOT met");
                    continue;
                }

                DebugSession.Log.Debug($"[Trigger] {trigger.Id}: conditions MET, executing {trigger.Effects.Count} effects");

                foreach (var effect in trigger.Effects)
                {
                    effect.Execute(context);
                }

                trigger.HasExecuted = true;
            }
            catch (Exception ex)
            {   
                DebugSession.Log.Error($"Trigger {trigger.Id} failed: {ex.Message}");
                DebugSession.Log.Debug($"Trigger {trigger.Id} failed: {ex}");
            }
        }
    }

    private static bool AllConditionsMet(ITrigger trigger, RuntimeContext context)
    {
        foreach (var c in trigger.Conditions)
            if (!c.Evaluate(context))
                return false;
        return true;
    }

    public void Disable(string triggerId)
    {
        if (_triggers.TryGetValue(triggerId, out var trigger))
            trigger.Enabled = false;
    }

    public void Enable(string triggerId)
    {
        if (_triggers.TryGetValue(triggerId, out var trigger))
            trigger.Enabled = true;
    }
}
