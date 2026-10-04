using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Map.Visibility;
using RTSEngine.Core.Players;
using RTSEngine.Core.State;

namespace RTSEngine.Core.Systems;

/// <summary>
/// Projects vision for every player from their own units and buildings, then
/// demotes tiles that are no longer covered back to Explored. No-op when fog
/// is disabled: there is no grid to update.
/// </summary>
public static class VisibilitySystem
{
    public static void Update(RuntimeContext context)
    {
        var world = context.World;

        if (!world.Fog.IsEnabled)
            return;

        ForgetDestroyedBuildings(world);

        foreach (var player in world.Players)
        {
            var state = world.Fog.GetState(player.Id);
            if (state == null)
                continue;

            state.BeginProjection();

            foreach (var unitId in player.UnitIds)
            {
                var unit = world.Entities.GetUnitById(unitId);
                if (unit == null || unit.IsDead)
                    continue;

                Project(state, unit.Position, unit.Definition.SightRange);
            }

            foreach (var buildingId in player.BuildingIds)
            {
                var building = world.Entities.GetBuildingById(buildingId);
                if (building == null || building.IsDead)
                    continue;

                Project(state, building.Position, building.Definition.SightRange);
            }

            state.DegradeStale();
            state.EndProjection();

            RememberVisibleEnemyBuildings(world, player, state);
        }
    }

    private static void Project(FogOfWarState state, GridPosition origin, int range)
    {
        foreach (var (dx, dy) in DiscProjector.Instance.Offsets(range))
            state.Project(new GridPosition(origin.X + dx, origin.Y + dy));
    }

    /// <summary>
    /// A dead enemy building leaves the remembered set here, so it stops being
    /// listed the tick it dies instead of lingering as rubble.
    /// </summary>
    private static void ForgetDestroyedBuildings(GameWorld world)
    {
        foreach (var building in world.Entities.Buildings)
        {
            if (!building.IsDead)
                continue;

            foreach (var player in world.Players)
                player.RememberedEnemyBuildingIds.Remove(building.Id);
        }
    }

    /// O(players × buildings) per tick; own buildings short-circuit before the
    /// grid read. Negligible next to the disc writes done in Update.
    private static void RememberVisibleEnemyBuildings(
        GameWorld world,
        Player player,
        FogOfWarState state)
    {
        foreach (var building in world.Entities.Buildings)
        {
            if (building.IsDead || building.OwnerId == player.Id)
                continue;

            if (state.Get(building.Position) == TileVisibility.Visible)
                player.RememberedEnemyBuildingIds.Add(building.Id);
        }
    }
}
