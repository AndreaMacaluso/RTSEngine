using RTSEngine.Core.Players;
using RTSEngine.Core.State;
using RTSEngine.Core.Map.Runtime;

namespace RTSEngine.Core.Systems;

public static class ScoreSystem
{
    private const int ScoreUpdateInterval = 60;

    private const int VillagerScore = 1;
    private const int MilitaryScore = 2;
    private const int BuildingScore = 3;
    private const int ResourceDivisor = 10;

    public static void Update(GameWorld world)
    {
        if (world.CurrentTick % ScoreUpdateInterval != 0)
        {
            return;
        }

        foreach (var player in world.Players)
        {
            if (player is not Player p)
            {
                continue;
            }

            p.Score = CalculateScore(world, p);
        }
    }

    private static int CalculateScore(GameWorld world, Player player)
    {
        int score = 0;

        foreach (var unitId in player.UnitIds)
        {
            var unit = world.Entities.GetUnitById(unitId);
            if (unit == null || unit.IsDead)
            {
                continue;
            }

            if (unit.Definition.CanGather)
            {
                score += VillagerScore;
            }
            else
            {
                score += MilitaryScore;
            }
        }

        foreach (var buildingId in player.BuildingIds)
        {
            var building = world.Entities.GetBuildingById(buildingId);
            if (building == null || building.IsDead)
            {
                continue;
            }

            score += BuildingScore;
        }

        int totalResources = player.Economy.Get(ResourceType.Wood)
            + player.Economy.Get(ResourceType.Food)
            + player.Economy.Get(ResourceType.Gold)
            + player.Economy.Get(ResourceType.Stone);

        score += totalResources / ResourceDivisor;

        return score;
    }
}
