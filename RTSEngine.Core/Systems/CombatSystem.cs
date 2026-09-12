using RTSEngine.Core.State;
using RTSEngine.Core.Entities;
using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Entities.Buildings;
using RTSEngine.Core.Entities.States;
using RTSEngine.Core.Helpers;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Actions;
using RTSEngine.Core.Players;

namespace RTSEngine.Core.Systems;

public static class CombatSystem
{
    public static void Update(RuntimeContext context)
    {
        GameWorld world = context.World;

        foreach (var unit in world.Entities.Units.Values)
        {
            if (unit.IsDead && unit.CurrentTask != EntityState.Decaying)
            {
                unit.CurrentTask = EntityState.Decaying;
                unit.DecayTicksRemaining = context.Engine.DecayTicks;
                unit.Combat.Clear();
                unit.Movement.PathQueue.Clear();
                unit.Movement.CurrentStep = null;

                var owner = world.GetPlayerById(unit.OwnerId);
                if (owner is Player player)
                {
                    PopulationActions.RemovePopulation(player, 1);
                }

                continue;
            }

            if (unit.CurrentTask == EntityState.Decaying)
            {
                continue;
            }

            unit.Combat.TickCooldown();

            switch (unit.Combat.Phase)
            {
                case CombatPhase.Guarding:
                    HandleGuarding(context, unit);
                    break;

                case CombatPhase.AttackMoving:
                    HandleAttackMoving(context, unit);
                    break;

                case CombatPhase.Idle:
                    if (unit.Definition.CanAttack
                        && unit.Definition.Category != EntityCategory.Villager)
                    {
                        HandleAutoAttack(world, unit);
                    }
                    break;

                case CombatPhase.MovingToTarget:
                    HandleMovingToTarget(context, unit);
                    break;

                case CombatPhase.Attacking:
                    HandleAttacking(context, unit);
                    break;
            }
        }

        HandleBuildingDeath(context);
        HandleBuildingCombat(context);
    }

    private static void HandleBuildingDeath(RuntimeContext context)
    {
        foreach (var building in context.World.Entities.Buildings.Values)
        {
            if (building.IsDead && building.CurrentTask != EntityState.Decaying)
            {
                building.CurrentTask = EntityState.Decaying;
                building.DecayTicksRemaining = context.Engine.DecayTicks;
                building.Combat.Clear();
            }
        }
    }

    private static Unit? FindFirstEnemyInRange(
        GameWorld world, int ownerId, GridPosition position, int range)
    {
        var player = world.GetPlayerById(ownerId);
        if (player == null) return null;

        foreach (var enemy in world.Entities.GetEnemyUnits(player))
        {
            int distance = WorldQueries.ChebyshevDistance(position, enemy.Position);
            if (distance <= range)
                return enemy;
        }

        return null;
    }

    private static void HandleBuildingCombat(RuntimeContext context)
    {
        GameWorld world = context.World;

        foreach (var building in world.Entities.Buildings.Values)
        {
            if (building.IsDead || !building.IsCompleted)
                continue;

            if (building.Combat.AttackRange <= 0)
                continue;

            building.Combat.TickCooldown();

            if (building.Combat.TargetEntityId.HasValue)
            {
                var target = world.Entities.GetUnitById(building.Combat.TargetEntityId.Value);

                if (target == null || target.IsDead)
                {
                    building.Combat.Clear();
                    building.CurrentTask = EntityState.Idle;
                    continue;
                }

                int distance = WorldQueries.ChebyshevDistance(
                    building.Position,
                    target.Position);

                if (distance > building.Combat.AttackRange)
                {
                    building.Combat.Clear();
                    building.CurrentTask = EntityState.Idle;
                    continue;
                }

                building.CurrentTask = EntityState.Attacking;

                if (!building.Combat.IsOnCooldown)
                {
                    SpawnProjectile(
                        context.Projectiles,
                        building.OwnerId,
                        building.Position,
                        target.Id,
                        target.Position,
                        DamageCalculator.CalculateDamage(building.Combat, target),
                        FixedPoint.FromFloat(2.0f));
                    building.Combat.ResetCooldown();
                }

                continue;
            }

            if (building.Combat.IsOnCooldown)
                continue;

            var enemy = FindFirstEnemyInRange(
                world, building.OwnerId, building.Position, building.Combat.AttackRange);

            if (enemy != null)
            {
                building.CurrentTask = EntityState.Attacking;

                SpawnProjectile(
                    context.Projectiles,
                    building.OwnerId,
                    building.Position,
                    enemy.Id,
                    enemy.Position,
                    DamageCalculator.CalculateDamage(building.Combat, enemy),
                    FixedPoint.FromFloat(2.0f));
                building.Combat.ResetCooldown();
            }
            else
            {
                building.CurrentTask = EntityState.Idle;
            }
        }
    }

    private static void HandleAttackMoving(RuntimeContext context, Unit unit)
    {
        GameWorld world = context.World;

        if (unit.Combat.TargetEntityId.HasValue)
        {
            HandleAttacking(context, unit);

            if (unit.Combat.Phase == CombatPhase.AttackMoving)
            {
                var enemy = FindFirstEnemyInRange(
                    world, unit.OwnerId, unit.Position, unit.Combat.AttackRange);
                if (enemy != null)
                    BeginAttack(world, unit, enemy.Id);
            }
            return;
        }

        var newEnemy = FindFirstEnemyInRange(
            world, unit.OwnerId, unit.Position, unit.Combat.AttackRange);
        if (newEnemy != null)
            BeginAttack(world, unit, newEnemy.Id);
    }

    private static void HandleAutoAttack(GameWorld world, Unit unit)
    {
        var enemy = FindFirstEnemyInRange(
            world, unit.OwnerId, unit.Position, unit.Combat.AttackRange);
        if (enemy != null)
            BeginAttack(world, unit, enemy.Id);
    }

    private static void HandleGuarding(RuntimeContext context, Unit unit)
    {
        GameWorld world = context.World;

        if (unit.Combat.TargetEntityId.HasValue)
        {
            HandleAttacking(context, unit);
            return;
        }

        var enemy = FindFirstEnemyInRange(
            world, unit.OwnerId, unit.Position, unit.Combat.AttackRange);
        if (enemy != null)
            BeginAttack(world, unit, enemy.Id);
    }

    private static void HandleMovingToTarget(
        RuntimeContext context,
        Unit unit)
    {
        GameWorld world = context.World;

        if (unit.Combat.TargetGroundPosition.HasValue)
        {
            var groundPos = unit.Combat.TargetGroundPosition.Value;
            int distance = WorldQueries.ChebyshevDistance(unit.Position, groundPos);

            if (distance <= unit.Combat.AttackRange)
            {
                EnterAttackingPhase(unit);
                return;
            }

            if (unit.Movement.PathQueue.Count == 0 && unit.Movement.CurrentStep == null)
                CommandSystem.AssignMoveTarget(unit, groundPos, context);

            return;
        }

        var target = ResolveTarget(world, unit);
        if (target == null) return;

        int targetDistance = WorldQueries.ChebyshevDistance(unit.Position, target.Position);

        if (targetDistance <= unit.Combat.AttackRange)
        {
            EnterAttackingPhase(unit);
            return;
        }

        if (unit.Movement.PathQueue.Count == 0 && unit.Movement.CurrentStep == null)
        {
            var adjacentTile = WorldQueries.FindClosestAdjacentWalkableTile(
                world, unit.Position, target.Position);

            if (adjacentTile is GridPosition tile)
                CommandSystem.AssignMoveTarget(unit, tile, context);
            else
                StopAttacking(unit);
        }
    }

    private static void HandleAttacking(
        RuntimeContext context,
        Unit unit)
    {
        GameWorld world = context.World;

        if (unit.Combat.TargetGroundPosition.HasValue)
        {
            HandleGroundAttack(context, unit);
            return;
        }

        var target = ResolveTarget(world, unit);
        if (target == null) return;

        int distance = WorldQueries.ChebyshevDistance(unit.Position, target.Position);

        if (distance > unit.Combat.AttackRange)
        {
            unit.Combat.Phase = CombatPhase.MovingToTarget;
            return;
        }

        if (unit.Combat.IsOnCooldown)
            return;

        if (unit.Combat.IsRanged)
        {
            SpawnProjectile(
                context.Projectiles,
                unit.OwnerId,
                unit.Position,
                target.Id,
                target.Position,
                DamageCalculator.CalculateDamage(unit.Combat, target),
                GetProjectileSpeed(unit),
                isSingleTarget: unit.Definition.Category != EntityCategory.Siege,
                splashRadius: unit.Definition.Category == EntityCategory.Siege ? FixedPoint.FromFloat(1.5f) : default);
        }
        else
        {
            target.TakeDamage(DamageCalculator.CalculateDamage(unit.Combat, target));
        }

        unit.Combat.ResetCooldown();
    }

    private static IHittable? ResolveTarget(GameWorld world, Unit unit)
    {
        if (unit.Combat.TargetEntityId is not int targetId)
        {
            StopAttacking(unit);
            return null;
        }

        var target = world.Entities.GetEntityById(targetId);

        if (target == null || target is not IHittable hittable || hittable.IsDead)
        {
            StopAttacking(unit);
            return null;
        }

        return hittable;
    }

    private static void EnterAttackingPhase(Unit unit)
    {
        unit.Combat.Phase = CombatPhase.Attacking;
        unit.Movement.PathQueue.Clear();
        unit.Movement.CurrentStep = null;
    }

    private static FixedPoint GetProjectileSpeed(Unit unit)
    {
        return unit.Definition.Category switch
        {
            EntityCategory.Siege => FixedPoint.FromFloat(0.8f),
            _ => FixedPoint.FromFloat(2.0f)
        };
    }

    private static void HandleGroundAttack(RuntimeContext context, Unit unit)
    {
        if (!unit.Combat.TargetGroundPosition.HasValue)
        {
            StopAttacking(unit);
            return;
        }

        var groundPos = unit.Combat.TargetGroundPosition.Value;
        int distance = WorldQueries.ChebyshevDistance(
            unit.Position,
            groundPos);

        if (distance > unit.Combat.AttackRange)
        {
            unit.Combat.Phase = CombatPhase.MovingToTarget;
            return;
        }

        if (unit.Combat.IsOnCooldown)
        {
            return;
        }

        SpawnProjectile(
            context.Projectiles,
            unit.OwnerId,
            unit.Position,
            targetEntityId: null,
            groundPos,
            unit.Combat.GetAttackDamage(true),
            GetProjectileSpeed(unit),
            isSingleTarget: false,
            splashRadius: FixedPoint.FromFloat(1.5f));

        unit.Combat.ResetCooldown();
    }

    private static void SpawnProjectile(
        ProjectileState projectiles,
        int ownerId,
        GridPosition origin,
        int? targetEntityId,
        GridPosition target,
        int damage,
        FixedPoint speed,
        bool isSingleTarget = true,
        FixedPoint splashRadius = default)
    {
        ProjectileSystem.SpawnProjectile(
            projectiles,
            ownerId: ownerId,
            originX: FixedPoint.FromGrid(origin.X),
            originY: FixedPoint.FromGrid(origin.Y),
            targetEntityId: targetEntityId,
            targetX: FixedPoint.FromGrid(target.X),
            targetY: FixedPoint.FromGrid(target.Y),
            damage: damage,
            speed: speed,
            isSingleTarget: isSingleTarget,
            splashRadius: splashRadius);
    }

    public static void BeginAttack(
        GameWorld world,
        Unit unit,
        int targetEntityId)
    {
        if (unit.Combat.Phase != CombatPhase.Guarding
            && unit.Combat.Phase != CombatPhase.AttackMoving)
        {
            unit.CurrentTask = EntityState.Attacking;
        }
        unit.Combat.TargetEntityId = targetEntityId;
        unit.Combat.Phase = CombatPhase.MovingToTarget;
        unit.Movement.PathQueue.Clear();
        unit.Movement.CurrentStep = null;
    }

    public static void StopAttacking(Unit unit)
    {
        unit.Combat.TargetEntityId = null;
        unit.Combat.TargetGroundPosition = null;
        unit.Combat.CooldownTicks = 0;

        if (unit.Combat.Phase == CombatPhase.Guarding
            || unit.Combat.Phase == CombatPhase.AttackMoving)
        {
            return;
        }

        unit.Combat.Phase = CombatPhase.Idle;
        unit.CurrentTask = EntityState.Idle;
    }
}
