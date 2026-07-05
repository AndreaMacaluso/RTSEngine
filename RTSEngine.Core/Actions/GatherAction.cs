using RTSEngine.Core.State;
using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Systems;
using RTSEngine.Core.Commands;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Players;
using RTSEngine.Core.Entities.Resources;
using RTSEngine.Core.Entities.States;
using RTSEngine.Core.Helpers;

namespace RTSEngine.Core.Actions;

public static class GatherActions
{
    public static void BeginMoveToResource(
    GameWorld world,
    Unit unit)
    {

        var resource = GetTargetResource(world,unit);

        if (resource == null)
        {
            return;
        }

        GridPosition? target =
            WorldQueries.FindClosestAdjacentWalkableTile(
                    world,
                    unit.Position,
                    resource.Position);
        if (target is not GridPosition destination)
        {
            return;
        }
        QueueMoveCommand(world,[unit.Id],destination);
    }

    public static void BeginMoveToDeposit(
    GameWorld world,
    Unit unit)
    {  
        if (unit.Gather.DepositPosition is not GridPosition deposit)
        {
            return;
        }
        var resource = GetTargetResource(world,unit);

        if (resource == null)
        {
            return;
        }

        GridPosition? target =
            WorldQueries.FindClosestAdjacentWalkableTile(
                    world,
                    unit.Position,
                    resource.Position);

        if (target is not GridPosition destination)
        {
            return;
        }
        QueueMoveCommand(world,[unit.Id],destination);
               
    }

    public static bool GatherOneTick(
    GameWorld world,
    Unit unit)
    {
        var resource = GetTargetResource(world,unit);

        if (resource == null)
            return true;

        if (resource.IsDepleted)
            return true;

        const int amount = 1;

        int collected =
            unit.Gather.AddLoad(amount);
        resource.Gather(collected);

        return unit.Gather.IsFull;
    }

    public static void DepositInventory(
    GameWorld world,
    Unit unit)
    {
        Player? owner = world.GetPlayerById(unit.OwnerId);

        if (owner is not Player player)
        {
            return;
        }
       
        if (unit.Gather.CarriedResource is not ResourceType gatheredResource)
        {
            return;
        }
        player.AddResource(gatheredResource,unit.Gather.CurrentLoad);
        unit.Gather.Clear();
    }

    public static bool CanContinueGathering(
        GameWorld world,
        Unit unit)
    {
        var resource = GetTargetResource(world,unit);

        return resource != null
            && !resource.IsDepleted;
    }

    public static void StopGathering(
    Unit unit)
    {
        unit.Gather.TargetResourceId = null;
        unit.Gather.DepositPosition = null;
        unit.Gather.CarriedResource = null;
        unit.Gather.Clear();
        unit.Gather.Phase = GatherPhase.None;

        unit.CurrentTask = UnitTask.Idle;
    }

    private static void QueueMoveCommand(
    GameWorld world,
    List<int> UnitIds,
    GridPosition destination)
    {
        world.AddCommand(new MoveCommand
        {
            UnitIds = UnitIds,
            Target = destination
        });
    }

    private static ResourceNode? GetTargetResource(
    GameWorld world,
    Unit unit)
    {
        if (unit.Gather.TargetResourceId is not int resourceId)
        {
            return null;
        }

        return world.GetResourceById(resourceId);
    }

}