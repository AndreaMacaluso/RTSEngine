using RTSEngine.Core.State;
using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Entities.States;
using RTSEngine.Core.Actions;
using RTSEngine.Core.Diagnostics;
using RTSEngine.Core.Helpers;
using RTSEngine.Core.Map.Runtime;

namespace RTSEngine.Core.Systems;

public static class GatherSystem
{
    public static void Update(GameWorld world)
    { 
        foreach (var entity in world.Entities)
        {
            if (entity is not Unit unit)
            {
                continue;
            } 
           
            if(!unit.Definition.CanGather )
            {
                continue;
            }
            switch(unit.Gather.Phase)
            {
                case GatherPhase.MovingToResource:
                    HandleMovingToResource(world, unit);
                    break;

                case GatherPhase.Gathering:
                    HandleGathering(world, unit);
                    break;

                case GatherPhase.MovingToDeposit:
                    HandleMovingToDeposit(world, unit);
                    break;

                case GatherPhase.Depositing:
                    HandleDepositing(world, unit);
                    break;
            }

            DebugSession.Log.Info(
            "Gather state",
            [
                ("Key", "GatherSystem_48"),
                ("Tick", world.CurrentTick),
                ("UnitId", unit.Id),
                ("Task", unit.CurrentTask),
                ("Phase", unit.Gather.Phase),
                ("Load", $"{unit.Gather.CurrentLoad}/{unit.Gather.Capacity}"),
                ("TargetResource", unit.Gather.TargetResourceId),
                ("CarriedResource", unit.Gather.CarriedResource),
                ("Deposit", unit.Gather.DepositPosition),
                ("PathNodes", unit.Movement.PathQueue.Count),
                ("Position", unit.Position),
                ("Destination", unit.Movement.TargetPosition)
            ]);
        }
    }

    //@toDo find a better place for this helper
    private static bool HasReachedDestination(
    GameWorld world,
    Unit unit)
    {
        switch (unit.Gather.Phase)
        {
            case GatherPhase.MovingToResource:
            {
                if (unit.Gather.TargetResourceId is not int resourceId)
                {
                    return false;
                }

                var resource = world.GetResourceById(resourceId);

                if (resource == null)
                {
                    return false;
                }

                return WorldQueries.IsAdjacent(
                    unit.Position,
                    resource.Position);
            }

            case GatherPhase.MovingToDeposit:
            {
                if (unit.Gather.DepositPosition is not GridPosition deposit)
                {
                    return false;
                }

                return WorldQueries.IsAdjacent(
                    unit.Position,
                    deposit);
            }

            default:
                return false;
        }
    }
    private static void HandleMovingToResource(
    GameWorld world,
    Unit unit)
    {   
        if (!GatherActions.CanContinueGathering(world, unit))
        {
            GatherActions.StopGathering(unit);
            return;
        }
        if (!HasReachedDestination(world,unit))
        {
            return;
        }

        unit.Gather.Phase = GatherPhase.Gathering;
    }
    private static void HandleGathering(
    GameWorld world,
    Unit unit)
    {
        // TODO:
        // GatherOneTick currently returns true only when the inventory
        // becomes full. In the future this should return a richer result
        // (enum GatherResult) to distinguish:
        // - InventoryFull
        // - ResourceDepleted
        // - CannotGather
        // - ContinueGathering
        if (!GatherActions.GatherOneTick(world, unit))
        {
            return;
        }

        unit.Gather.Phase = GatherPhase.MovingToDeposit;

        
        if (!GatherActions.BeginMoveToDeposit(world, unit))
        {
            GatherActions.StopGathering(unit);
        }
    }
    private static void HandleMovingToDeposit(
    GameWorld world,
    Unit unit)
    {   
        if (!HasReachedDestination(world,unit))
        {
            return;
        }

        unit.Gather.Phase = GatherPhase.Depositing;
    }
    private static void HandleDepositing(
    GameWorld world,
    Unit unit)
    {
        GatherActions.DepositInventory(
            world,
            unit);

        if (GatherActions.CanContinueGathering(world, unit))
        {
            unit.Gather.Phase =
                GatherPhase.MovingToResource;

            GatherActions.BeginMoveToResource(
                world,
                unit);
        }
        else
        {
            GatherActions.StopGathering(unit);
            unit.Gather.Clear();
        }
    }
        

}