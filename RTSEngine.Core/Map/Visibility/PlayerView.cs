using RTSEngine.Core.Entities.Buildings;
using RTSEngine.Core.Entities.Resources;
using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Helpers;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.State;

namespace RTSEngine.Core.Map.Visibility;

/// <summary>
/// The world as one player sees it. Every property reads the live world, so a
/// view is valid only for the tick you ask it on.
///
/// Test/debug only: it allocates a list per property read, so the network and
/// the replay go through FogQueries instead.
///
/// Own units always; enemy units only on Visible, never on Explored; enemy
/// buildings visible plus the remembered ones minus destroyed; resources on
/// Visible or Explored, never Hidden.
/// </summary>
public sealed class PlayerView
{
    private readonly GameWorld _world;

    public int PlayerId { get; }
    public VisionScope Scope { get; }

    internal PlayerView(GameWorld world, int playerId)
    {
        _world = world;
        PlayerId = playerId;
        Scope = world.Fog.ScopeFor(playerId);
    }

    public IReadOnlyList<Unit> Units
    {
        get
        {
            var result = new List<Unit>();

            foreach (var unit in _world.Entities.Units)
            {
                if (unit.IsDead)
                    continue;

                if (unit.OwnerId == PlayerId || FogQueries.IsVisible(_world, Scope, unit.Position))
                    result.Add(unit);
            }

            return result;
        }
    }

    public IReadOnlyList<Building> Buildings
    {
        get
        {
            var known = _world.Fog.IsEnabled
                ? FogQueries.GetKnownEnemyBuildings(_world, Scope).ToList()
                : null;

            var result = new List<Building>();

            foreach (var building in _world.Entities.Buildings)
            {
                if (building.IsDead)
                    continue;

                if (building.OwnerId == PlayerId || known is null || known.Contains(building))
                    result.Add(building);
            }

            return result;
        }
    }

    public IReadOnlyList<ResourceNode> Resources
    {
        get
        {
            var result = new List<ResourceNode>();

            foreach (var resource in _world.Entities.Resources)
            {
                if (resource.IsDepleted)
                    continue;

                if (FogQueries.IsSeen(_world, Scope, resource.Position))
                    result.Add(resource);
            }

            return result;
        }
    }

    public TileVisibility At(GridPosition position)
        => _world.Fog.Get(PlayerId, position);

    public bool IsVisible(GridPosition position)
        => FogQueries.IsVisible(_world, Scope, position);

    public bool IsSeen(GridPosition position)
        => FogQueries.IsSeen(_world, Scope, position);
}
