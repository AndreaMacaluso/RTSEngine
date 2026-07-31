using RTSEngine.Core.Entities.Buildings;
using RTSEngine.Core.Entities.Definitions;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Entities.Rules;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Players;
using RTSEngine.Core.State;

namespace RTSEngine.Core.Actions;

public static class BuildingPlacementActions
{
    public static Building? PlaceFoundation(
        GameWorld world,
        Player player,
        BuildingDefinition definition,
        GridPosition position)
    {
        if (!BuildingPlacementRules.CanPlace(
            world,
            definition,
            position))
        {
            return null;
        }

        if (!EconomyActions.TryPay(
        player,
        definition.Costs))
        {
            return null;
        }

        var building = BuildingFactory.Create(
            definition,
            player.Id,
            position);

        building.IsCompleted = false;
        building.ConstructionProgress = 0;

        world.AddEntity(building);

        return building;
    }

    public static void RemoveFoundation(
        GameWorld world,
        Player player,
        Building building,
        bool refundResources = true)
    {
        if (refundResources)
        {
            EconomyActions.Refund(
                player,
                building.Definition.Costs);
        }

        world.RemoveEntity(building);
    }
}