using RTSEngine.Core.Entities.Runtime;

namespace RTSEngine.Core.Triggers;

public interface ITriggerEffect
{
    void Execute(RuntimeContext context);
}
