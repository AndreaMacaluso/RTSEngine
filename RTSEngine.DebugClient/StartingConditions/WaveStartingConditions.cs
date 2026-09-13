using RTSEngine.Core.Helpers;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Players;
using RTSEngine.Core.Entities.Runtime;

namespace RTSEngine.DebugClient.StartingConditions;

public static class WaveStartingConditions
{
    private static readonly int[] TowerYPositions = [5, 15, 25, 35];

    public static void Create(RuntimeContext context)
    {
        foreach (var player in context.World.Players)
        {
            var spawn = context.World.Spawns
                .First(s => s.PlayerId == player.Id);

            SpawnTowers(
                context,
                player.Id,
                new GridPosition(spawn.X, spawn.Y));

            GrantStartingResources(player);
        }
    }

    private static void SpawnTowers(
        RuntimeContext context,
        int ownerId,
        GridPosition spawnCenter)
    {
        bool isLeftSide = spawnCenter.X < context.World.Map.Width / 2;
        int towerX = isLeftSide ? 3 : 36;

        foreach (var y in TowerYPositions)
        {
            var position = new GridPosition(towerX, y);
            SpawnWatchTower(context, ownerId, position);
        }
    }

    private static void SpawnWatchTower(
        RuntimeContext context,
        int ownerId,
        GridPosition position)
    {
        var definition = context.BuildingRepository.Get(EntityIds.WatchTower);

        var building = BuildingFactory.Create(
            definition,
            ownerId,
            position);

        building.IsCompleted = true;
        building.Health.CurrentHealth = definition.MaxHealth;

        var player = context.World.GetPlayerById(ownerId)
            ?? throw new InvalidOperationException(
                $"Cannot spawn a watch tower for unknown player {ownerId}.");

        context.World.Entities.Add(building, player);
    }

    private static void GrantStartingResources(Player player)
    {
        player.Economy.Add(ResourceType.Wood, 200);
        player.Economy.Add(ResourceType.Food, 200);
        player.Economy.Add(ResourceType.Gold, 100);
        player.Economy.Add(ResourceType.Stone, 100);
    }
}
