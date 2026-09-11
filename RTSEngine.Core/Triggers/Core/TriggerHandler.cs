using RTSEngine.Core.Diagnostics;
using RTSEngine.Core.Entities.Runtime;

namespace RTSEngine.Core.Triggers;

public sealed class TriggerHandler
{
    private readonly Dictionary<string, ITrigger> _triggers = new();

    public void RegisterTrigger(ITrigger trigger)
    {
        _triggers[trigger.Id] = trigger;
    }

    public void Update(RuntimeContext context)
    {
        foreach (var trigger in _triggers.Values.ToList())
        {
            try
            {
                if (!trigger.Enabled) continue;

                if (!trigger.Looping && trigger.HasExecuted) continue;

                DebugSession.Log.Debug($"[Trigger] Evaluating: {trigger.Id} (Tick: {context.World.CurrentTick})");

                // If no conditions, trigger always fires (by design - LINQ All() returns true for empty collections)
                var allConditionsMet = trigger.Conditions.Count == 0 || trigger.Conditions.All(c => c.Evaluate(context));
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
            }
        }
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
