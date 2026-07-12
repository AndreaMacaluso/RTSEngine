using RTSEngine.Core.Entities.Buildings;
using RTSEngine.Core.Entities.States;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Helpers;
using RTSEngine.Core.State;
using RTSEngine.Core.Commands;

namespace RTSEngine.Core.Actions;

public static class ConstructionActions
{
    public static void BeginMoveToConstruction(
        GameWorld world,
        Unit unit)
    {
        if (unit.Build.BuildingId is not int buildingId)
        {
            return;
        }

        Building? building =
            world.GetBuildingById(buildingId);

        if (building == null)
        {
            return;
        }

        GridPosition? target =
            WorldQueries.FindClosestAdjacentWalkableTile(
                world,
                unit.Position,
                building.Position);

        if (target is not GridPosition destination)
        {
            return;
        }

        world.AddCommand(new MoveCommand
        {
            UnitIds = [unit.Id],
            Target = destination
        });
    }

    public static bool BuildOneTick(
        GameWorld world,
        Unit unit)
    {
        if (unit.Build.BuildingId is not int buildingId)
        {
            return true;
        }

        Building? building =
            world.GetBuildingById(buildingId);

        if (building == null)
        {
            return true;
        }

        building.ConstructionProgress++;

        return
            building.ConstructionProgress >=
            building.Definition.BuildTimeTicks;
    }

    public static void CompleteConstruction(
        GameWorld world,
        Unit unit)
    {
        if (unit.Build.BuildingId is not int buildingId)
        {
            return;
        }

        Building? building =
            world.GetBuildingById(buildingId);

        if (building == null)
        {
            return;
        }

        building.IsCompleted = true;
        building.CurrentHealth =
            building.Definition.MaxHealth;
    }

    public static bool CanContinueBuilding(
        GameWorld world,
        Unit unit)
    {
        if (unit.Build.BuildingId is not int buildingId)
        {
            return false;
        }

        Building? building =
            world.GetBuildingById(buildingId);

        return
            building != null &&
            !building.IsCompleted;
    }

    public static void StopBuilding(
        Unit unit)
    {
        unit.CurrentTask = UnitTask.Idle;

        unit.Build.BuildingId = null;
        unit.Build.Phase = BuildPhase.None;
    }
}