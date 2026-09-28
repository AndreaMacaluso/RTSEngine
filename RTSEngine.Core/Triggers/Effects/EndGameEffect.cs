using RTSEngine.Core.Entities.Runtime;

namespace RTSEngine.Core.Triggers;

public sealed class EndGameEffect : ITriggerEffect
{
    public void Execute(RuntimeContext context)
    {
        context.World.Finish();
    }
}
