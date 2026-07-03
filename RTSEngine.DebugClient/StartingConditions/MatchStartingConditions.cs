using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Players;
using RTSEngine.DebugClient.Bootstrap;

namespace RTSEngine.DebugClient.StartingConditions;

public static class MatchStartingConditions
{
    public static void CreateStandard(
        SimulationContext context)
    {
        foreach (Player player in context.World.Players)
        {
            var spawn = context.World.Spawns
                .First(s => s.PlayerId == player.Id);

            CreateStartingBase(
                context,
                player.Id,
                new GridPosition(spawn.X, spawn.Y));
        }
    }

    private static void CreateStartingBase(
        SimulationContext context,
        int ownerId,
        GridPosition center)
    {
        EntitySpawner.SpawnTownCenter(
            context,
            ownerId,
            center);

        EntitySpawner.SpawnVillager(
            context,
            ownerId,
            new GridPosition(center.X - 2, center.Y));

        EntitySpawner.SpawnVillager(
            context,
            ownerId,
            new GridPosition(center.X, center.Y - 2));

        EntitySpawner.SpawnVillager(
            context,
            ownerId,
            new GridPosition(center.X + 2, center.Y));
    }
}