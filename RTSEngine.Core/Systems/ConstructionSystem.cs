using RTSEngine.Core.State;
using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Entities.States;
using RTSEngine.Core.Helpers;
using RTSEngine.Core.Actions;
using RTSEngine.Core.Map.Runtime;

namespace RTSEngine.Core.Systems;

public class ConstructionSystem
{

    public static void Update(GameWorld world)
    { 
        foreach (var entity in world.Entities)
        {
            if (entity is not Unit unit)
            {
                continue;
            } 
           
            if(!unit.Definition.CanBuild )
            {
                continue;
            }
            switch(unit.Build.Phase)
            {
                case BuildPhase.MovingToConstruction:
                    HandleMovingToConstruction(world, unit);
                    break;

                case BuildPhase.Constructing:
                    HandleConstructing(world, unit);
                    break;
            }
        }
    }

    
    private static void HandleMovingToConstruction(
    GameWorld world,
    Unit unit)
    {   
        if (unit.Build.BuildPosition is not GridPosition destination)
            {
                return;
            }

        if (!WorldQueries.HasReachedDestination(unit, destination))
            {
                return;
            }

            unit.Build.Phase = BuildPhase.Constructing;
    }

    private static void HandleConstructing(
    GameWorld world,
    Unit unit)
    {
        if (!ConstructionActions.BuildOneTick(world, unit))
        {
            return;
        }

        ConstructionActions.CompleteConstruction(
            world,
            unit);

        ConstructionActions.StopBuilding(unit);
    }
}