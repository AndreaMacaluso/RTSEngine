using RTSEngine.Core.Entities.Runtime;

namespace RTSEngine.Core.Triggers;

public interface ITriggerCondition
{
    bool Evaluate(RuntimeContext context);
}
