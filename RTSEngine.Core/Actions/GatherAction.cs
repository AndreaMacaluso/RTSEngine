using RTSEngine.Core.State;
using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Systems;
using RTSEngine.Core.Commands;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Players;
using RTSEngine.Core.Entities.Resources;
using RTSEngine.Core.Entities.States;
using RTSEngine.Core.Helpers;
using RTSEngine.Core.Diagnostics;

namespace RTSEngine.Core.Actions;

public static class GatherActions
{
    public static bool BeginMoveToResource(
    GameWorld world,
    Unit unit)
    {
        var resource = GetTargetResource(world, unit);

        if (resource == null)
        {
            return false;
        }

        unit.Gather.DepositPosition = null;

        GridPosition? target =
            WorldQueries.FindClosestAdjacentWalkableTile(
                world,
                unit.Position,
                resource.Position);

        if (target is not GridPosition destination)
        {
            return false;
        }

        QueueMoveCommand(
            world,
            [unit.Id],
            destination);

        return true;
    }

    public static bool BeginMoveToDeposit(
    GameWorld world,
    Unit unit)
    {

        DebugSession.Log.Info(
        "BeginMoveToDeposit",
        [
            ("Unit", unit.Id),
            ("CurrentPosition", unit.Position),
            ("CarriedResource", unit.Gather.CarriedResource)
        ]);
        if (unit.Gather.CarriedResource is not ResourceType resourceType)
        {
            return false;
        }

        var deposit = WorldQueries.FindClosestDeposit(
            world,
            unit.OwnerId,
            unit.Position,
            resourceType);
        DebugSession.Log.Info(
        "Deposit found",
        [
            ("Deposit", deposit?.Position),
            ("UnitPosition", unit.Position)
        ]);
        if (deposit == null)
        {
            return false;
        }

        unit.Gather.DepositPosition = deposit.Position;

        var target = WorldQueries.FindClosestAdjacentWalkableTile(
            world,
            unit.Position,
            deposit.Position);

        if (target is not GridPosition destination)
        {
            return false;
        }

        QueueMoveCommand(world, [unit.Id], destination);
        return true;
    }

    public static GatherResult GatherOneTick(
    GameWorld world,
    Unit unit)
    {
        var resource = GetTargetResource(world,unit);

        if (resource == null)
        {
            return GatherResult.InvalidTarget;
        }

        if (resource.IsDepleted)
        {
            return GatherResult.ResourceDepleted;
        }
        //start gather
        if (unit.Gather.IsEmpty)
        {
            unit.Gather.CarriedResource = resource.ResourceType;
        }

        const int amount = 1;

        int collected = unit.Gather.AddLoad(amount);
        resource.Gather(collected);

        if (unit.Gather.IsFull)
        {
            return GatherResult.InventoryFull;
        }

        if (resource.IsDepleted)
        {
            return GatherResult.ResourceDepleted;
        }
        return GatherResult.ContinueGathering;
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
        var resource = GetTargetResource(world, unit);

        if (resource != null && !resource.IsDepleted)
        {
            return true;
        }

        if (unit.Gather.CarriedResource is not ResourceType resourceType)
        {
            return false;
        }

        var deposit = WorldQueries.FindClosestDeposit(
            world,
            unit.OwnerId,
            unit.Position,
            resourceType);

        if (deposit == null)
        {
            return false;
        }

        var nextResource = WorldQueries.FindClosestResource(
            world,
            unit.Position,
            resourceType);

        if (nextResource == null)
        {
            return false;
        }

        var target = WorldQueries.FindClosestAdjacentWalkableTile(
                    world,
                    unit.Position,
                    nextResource.Position);
        
        if (target == null)
        {
            return false;
        }


        unit.Gather.TargetResourceId = nextResource.Id;

        unit.CurrentTask = UnitTask.Gathering;
        unit.Gather.Phase = GatherPhase.MovingToResource;
        unit.Gather.CarriedResource = resourceType;
        CommandSystem.AssignMoveTarget(unit, target.Value, world);

        return true;
    }

   public static void StopGathering(Unit unit)
    {
        unit.Gather.TargetResourceId = null;
        unit.Gather.DepositPosition = null;
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