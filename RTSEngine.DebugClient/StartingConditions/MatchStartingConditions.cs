using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Players;
using RTSEngine.Core.Entities.Runtime;

namespace RTSEngine.DebugClient.StartingConditions;

public static class MatchStartingConditions
{
    public static void CreateStandard(
        RuntimeContext context)
    {
        foreach (var player in context.World.Players)
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
        RuntimeContext context,
        int ownerId,
        GridPosition spawnCorner)
    {
        var definition = context.BuildingRepository.Get("town_center");
        var halfW = definition.Width / 2;
        var halfH = definition.Height / 2;
        var center = new GridPosition(
            spawnCorner.X + halfW,
            spawnCorner.Y + halfH);

        EntitySpawner.SpawnTownCenter(
            context,
            ownerId,
            center);

        EntitySpawner.SpawnVillager(
            context,
            ownerId,
            new GridPosition(center.X + 4, center.Y));

        EntitySpawner.SpawnVillager(
            context,
            ownerId,
            new GridPosition(center.X - 4, center.Y));

        EntitySpawner.SpawnVillager(
            context,
            ownerId,
            new GridPosition(center.X, center.Y + 4));
    }
}
