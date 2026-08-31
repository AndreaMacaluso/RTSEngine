using RTSEngine.Core.State;
using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Entities.States;
using RTSEngine.Core.Helpers;
using RTSEngine.Core.Actions;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Commands;

namespace RTSEngine.Core.Systems;

public static class ConstructionSystem
{

    public static void Update(RuntimeContext context)
    { 

        var world = context.World;
        foreach (var unit in world.Entities.Units.Values) {
            
            if(!unit.Definition.CanBuild )
            {
                continue;
            }
            switch(unit.Build.Phase)
            {
                case BuildPhase.MovingToConstruction:
                    HandleMovingToConstruction(context, unit);
                    break;

                case BuildPhase.Constructing:
                    HandleConstructing(context, unit);
                    break;
            }
        }
    }

    
    private static void HandleMovingToConstruction(
    RuntimeContext context,
    Unit unit)
    {   
        if (unit.Movement.NeedsRepath)
        {
            unit.Movement.NeedsRepath = false;

            if (!ConstructionActions.BeginMoveToConstruction(
                context,
                unit))
            {
                ConstructionActions.StopBuilding(unit);
            }

            return;
        }
        if (unit.Build.BuildingId is not int buildingId)
            {
                return;
            }

        var building = context.World.Entities.GetBuildingById(buildingId);
        if (building is null)
            {
                ConstructionActions.StopBuilding(unit);
                return;
            }

        if (!WorldQueries.HasReachedDestination(unit, building.Position))
            {
                return;
            }

            unit.Build.Phase = BuildPhase.Constructing;
    }

    private static void HandleConstructing(
    RuntimeContext context,
    Unit unit)
    {
        if (unit.Build.BuildingId is not int buildingId)
        {
            ConstructionActions.StopBuilding(unit);
            return;
        }

        var building = context.World.Entities.GetBuildingById(buildingId);

        if (building == null || building.IsCompleted)
        {
            ConstructionActions.StopBuilding(unit);
            return;
        }

        if (!ConstructionActions.BuildOneTick(context.World, unit))
        {
            return;
        }

        ConstructionActions.CompleteConstruction(
            context.World,
            unit,
            context.Settings.PopulationCap);

        ConstructionActions.StopBuilding(unit);
    }
}
