using RTSEngine.Core.Entities;
using RTSEngine.Core.State;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Helpers;

namespace RTSEngine.Core.Systems;

public static class ProjectileSystem
{
    public static void SpawnProjectile(
        ProjectileState state,
        int ownerId,
        FixedPoint originX,
        FixedPoint originY,
        int? targetEntityId,
        FixedPoint targetX,
        FixedPoint targetY,
        int damage,
        FixedPoint speed,
        bool isSingleTarget = true,
        FixedPoint splashRadius = default)
    {
        state.Add(new Projectile
        {
            Id = state.NextId(),
            OwnerId = ownerId,
            X = originX,
            Y = originY,
            TargetEntityId = targetEntityId,
            TargetX = targetX,
            TargetY = targetY,
            Damage = damage,
            Speed = speed,
            IsSingleTarget = isSingleTarget,
            SplashRadius = splashRadius,
            IsActive = true
        });
    }

    public static void Update(RuntimeContext context)
    {
        var world = context.World;
        var state = context.Projectiles;

        for (int i = state.Projectiles.Count - 1; i >= 0; i--)
        {
            var projectile = state.Projectiles[i];

            if (!projectile.IsActive)
            {
                state.RemoveAt(i);
                continue;
            }

            projectile.MoveTowardsTarget();

            if (projectile.HasReachedTarget)
            {
                ApplyDamage(world, projectile);
                state.RemoveAt(i);
            }
        }
    }

    private static void ApplyDamage(GameWorld world, Projectile projectile)
    {
        if (projectile.IsSingleTarget)
        {
            ApplySingleTargetDamage(world, projectile);
        }
        else
        {
            ApplySplashDamage(world, projectile);
        }
    }

    private static void ApplySingleTargetDamage(GameWorld world, Projectile projectile)
    {
        if (projectile.TargetEntityId.HasValue)
        {
            var target = world.Entities.GetEntityById(projectile.TargetEntityId.Value);
            if (target is IHittable hittable && !hittable.IsDead)
            {
                hittable.TakeDamage(projectile.Damage);
            }
        }
    }

    private static void ApplySplashDamage(GameWorld world, Projectile projectile)
    {
        int damage = projectile.Damage;
        int splashSq = projectile.SplashRadius.Raw * projectile.SplashRadius.Raw;

        foreach (var unit in world.Entities.Units.Values)
        {
            if (unit.IsDead) continue;

            int distSq = DistanceToProjectileSquared(projectile, unit.Position);
            if (distSq <= splashSq)
            {
                int actualDamage = distSq < 250 ? damage : damage / 2;
                unit.TakeDamage(actualDamage);
            }
        }

        foreach (var building in world.Entities.Buildings.Values)
        {
            if (building.IsDead) continue;

            int distSq = DistanceToProjectileSquared(projectile, building.Position);
            if (distSq <= splashSq)
            {
                int actualDamage = distSq < 250 ? damage : damage / 2;
                building.TakeDamage(actualDamage);
            }
        }
    }

    private static int DistanceToProjectileSquared(Projectile projectile, GridPosition position)
    {
        FixedPoint pos = new FixedPoint(position.X * 1000 + 500);
        FixedPoint posY = new FixedPoint(position.Y * 1000 + 500);
        return FixedPoint.DistanceSquared(projectile.X, projectile.Y, pos, posY);
    }
}
