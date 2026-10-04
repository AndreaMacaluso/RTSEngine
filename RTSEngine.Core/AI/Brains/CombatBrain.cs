using RTSEngine.Core.AI.Actions;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Helpers;
using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Players;
using RTSEngine.Core.Settings;
using RTSEngine.Core.State;

namespace RTSEngine.Core.AI.Brains;

public class CombatBrain : AIBrain
{
    private readonly List<(Unit Unit, int EnemyId)> _pendingAttacks = [];
    private readonly List<(Unit Unit, GridPosition Target)> _pendingMoves = [];

    protected override string Think(RuntimeContext context, Player player)
    {
        // Built once: every query below reads this player's grid.
        var scope = context.World.Fog.ScopeFor(player.Id);

        if (!FogQueries.HasEnemies(context.World, scope))
            return BrainActions.None;

        var idleMilitary = UnitQueries.FindIdleMilitary(context.World, player);

        if (idleMilitary.Count == 0)
            return BrainActions.None;

        var enemyTC = FogQueries.FindEnemyBuilding(context.World, scope, EntityIds.TownCenter);

        foreach (var unit in idleMilitary)
        {
            var nearestEnemy = FogQueries.FindNearestEnemyEntity(
                context.World, scope, unit.Position);

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
            CombatAIActions.AttackTarget(context.CommandQueue, unit, enemyId);
        }

        foreach (var (unit, target) in _pendingMoves)
        {
            CombatAIActions.MoveToTarget(context.CommandQueue, unit, target);
        }

        _pendingAttacks.Clear();
        _pendingMoves.Clear();
    }
}
