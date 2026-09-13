using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.State;

namespace RTSEngine.Core.Triggers;

public sealed class EndGameEffect : ITriggerEffect
{
    public void Execute(RuntimeContext context)
    {
        context.World.Finish();
    }
}
