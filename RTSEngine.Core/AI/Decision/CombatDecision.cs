using RTSEngine.Core.AI.Actions;
using RTSEngine.Core.AI.Planning;
using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Helpers;
using RTSEngine.Core.Players;
using RTSEngine.Core.State;

namespace RTSEngine.Core.AI.Decisions;

public static class CombatDecision
{
    private const int AggroRange = 15;
    private const int AttackRange = 20;

    public static void Execute(
        GameWorld world,
        Player player)
    {
        if (!CombatPlanner.HasEnemies(world, player))
        {
            return;
        }

        var idleMilitary = world.Entities
            .OfType<Unit>()
            .Where(u =>
                u.OwnerId == player.Id
                && !u.IsDead
                && u.Definition.CanAttack
                && u.CurrentTask == UnitTask.Idle)
            .ToList();

        var enemyTC = CombatPlanner.FindEnemyTownCenter(world, player);

        foreach (var unit in idleMilitary)
        {
            var nearestEnemy = CombatPlanner.FindNearestEnemyEntity(
                world,
                player,
                unit);

            if (nearestEnemy.HasValue)
            {
                int distanceToEnemy = WorldQueries.ChebyshevDistance(
                    unit.Position,
                    nearestEnemy.Value.Entity.Position);

                if (distanceToEnemy <= AggroRange)
                {
                    CombatAIActions.AttackTarget(
                        world,
                        unit,
                        nearestEnemy.Value.Entity.Id);
                    continue;
                }
            }

            if (enemyTC != null)
            {
                int distanceToTC = WorldQueries.ChebyshevDistance(
                    unit.Position,
                    enemyTC.Position);

                if (distanceToTC > AggroRange)
                {
                    CombatAIActions.MoveToTarget(
                        world,
                        unit,
                        enemyTC.Position);
                }
            }
        }
    }
}
