using  RTSEngine.Core.State;
namespace RTSEngine.Core.Systems;
public static class ResourceCleanupSystem
{
    public static void Update(GameWorld world)
    {
        foreach (var resource in world.Resources
                     .Where(r => r.IsDepleted)
                     .ToList())
        {
            world.RemoveResources(resource);
        }
    }
}