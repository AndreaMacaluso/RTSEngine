using RTSEngine.Core.Helpers;
using RTSEngine.Core.State;
namespace RTSEngine.Core.Systems;
public static class ResourceCleanupSystem
{
    public static void Update(GameWorld world)
    {
        foreach (var resource in WorldQueries.FindDepletedResources(world))
        {
            world.RemoveResource(resource);
        }
    }
}
