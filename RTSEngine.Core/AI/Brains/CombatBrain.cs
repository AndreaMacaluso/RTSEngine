using RTSEngine.Core.AI.Actions;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Helpers;
using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Players;
using RTSEngine.Core.State;

namespace RTSEngine.Core.AI.Brains;

public class CombatBrain : AIBrain
{
    private readonly List<(Unit Unit, int EnemyId)> _pendingAttacks = [];
    private readonly List<(Unit Unit, GridPosition Target)> _pendingMoves = [];

    protected override string Think(RuntimeContext context, Player player)
    {
        if (!WorldQueries.HasEnemies(context.World, player))
            return BrainActions.None;

        var idleMilitary = UnitQueries.FindIdleMilitary(context.World, player);

        if (idleMilitary.Count == 0)
            return BrainActions.None;

        var enemyTC = WorldQueries.FindEnemyBuilding(context.World, player, EntityIds.TownCenter);

        foreach (var unit in idleMilitary)
        {
            var nearestEnemy = WorldQueries.FindNearestEnemyEntity(
                context.World, player, unit.Position);

            if (nearestEnemy.HasValue)
            {
                int distanceToEnemy = WorldQueries.ChebyshevDistance(
                    unit.Position, nearestEnemy.Value.Entity.Position);

                if (distanceToEnemy <= GameConfig.AggroRange)
                {
                    _pendingAttacks.Add((unit, nearestEnemy.Value.Entity.Id));
                    continue;
                }
            }

            if (enemyTC != null)
            {
                int distanceToTC = WorldQueries.ChebyshevDistance(
                    unit.Position, enemyTC.Position);

                if (distanceToTC > GameConfig.AggroRange)
                {
                    _pendingMoves.Add((unit, enemyTC.Position));
                }
            }
        }

        return (_pendingAttacks.Count > 0 || _pendingMoves.Count > 0)
            ? BrainActions.EngageEnemies
            : BrainActions.None;
    }

    protected override void ExecutePlan(RuntimeContext context, Player player, string action)
    {
        if (action == BrainActions.None) return;

        foreach (var (unit, enemyId) in _pendingAttacks)
        {
            CombatAIActions.AttackTarget(context.World, unit, enemyId);
        }

        foreach (var (unit, target) in _pendingMoves)
        {
            CombatAIActions.MoveToTarget(context.World, unit, target);
        }

        _pendingAttacks.Clear();
        _pendingMoves.Clear();
    }
}
