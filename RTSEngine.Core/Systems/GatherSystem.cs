using RTSEngine.Core.State;
using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Entities.States;
using RTSEngine.Core.Actions;
using RTSEngine.Core.Diagnostics;
using RTSEngine.Core.Helpers;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Commands;

namespace RTSEngine.Core.Systems;

public static class GatherSystem
{
    private const int StuckThreshold = 2;
    private const int RetryInterval = 2;
    private const int MaxWaitTicks = 4;

    public static void Update(RuntimeContext context)
    { 
        var world = context.World;
        var commandQueue = context.CommandQueue;

        foreach (var unit in world.Entities.Units.Values)
        {
           
            if(!unit.Definition.CanGather )
            {
                continue;
            }
            switch(unit.Gather.Phase)
            {
                case GatherPhase.MovingToResource:
                    HandleMovingToResource(context, commandQueue, unit);
                    break;

                case GatherPhase.Gathering:
                    HandleGathering(context, commandQueue, unit);
                    break;

                case GatherPhase.MovingToDeposit:
                    HandleMovingToDeposit(context, commandQueue, unit);
                    break;

                case GatherPhase.WaitingForDeposit:
                    HandleWaitingForDeposit(context, commandQueue, unit);
                    break;

                case GatherPhase.Depositing:
                    HandleDepositing(context, commandQueue, unit);
                    break;
            }

            DebugSession.Log.Debug(
            "Gather state",
            [
                //("Key", "GatherSystem_48"),
                ("Tick", world.CurrentTick),
                ("UnitId", unit.Id),
                ("Task", unit.CurrentTask),
                ("Phase", unit.Gather.Phase),
                // ("Load", $"{unit.Gather.CurrentLoad}/{unit.Gather.Capacity}"),
                // ("TargetResource", unit.Gather.TargetResourceId),
                // ("CarriedResource", unit.Gather.CarriedResource),
                // ("Deposit", unit.Gather.DepositPosition),
                // ("PathNodes", unit.Movement.PathQueue.Count),
                // ("Position", unit.Position),
                // ("Destination", unit.Movement.Destination)
            ]);
        }
    }
    private static void HandleMovingToResource(
    RuntimeContext context,
    ICommandQueue commandQueue,
    Unit unit)
    {   

        if (unit.Movement.NeedsRepath)
        {
            unit.Movement.NeedsRepath = false;

            if (!GatherActions.BeginMoveToResource(context, unit))
            {
                GatherActions.StopGathering(unit);
            }

            return;
        }
        if (!GatherActions.CanContinueGathering(context.World, unit))
        {
            GatherActions.StopGathering(unit);
            return;
        }

        if (unit.Gather.TargetResourceId is not int resourceId)
        {
            return;
        }

        var resource = context.World.Entities.GetResourceById(resourceId);

        if (resource == null)
        {
            GatherActions.StopGathering(unit);
            return;
        }

        if (!WorldQueries.HasReachedDestination(unit, resource.Position))
        {
            return;
        }

        unit.Gather.Phase = GatherPhase.Gathering;
    }
    private static void HandleGathering(
    RuntimeContext context,
    ICommandQueue commandQueue,
    Unit unit)
    {
        var world = context.World;
        switch (GatherActions.GatherOneTick(world, unit))
        {
            case GatherResult.ContinueGathering:
                return;

            case GatherResult.InventoryFull:

                unit.Gather.Phase = GatherPhase.MovingToDeposit;

                if (!GatherActions.BeginMoveToDeposit(context, unit))
                {
                    unit.Gather.Phase = GatherPhase.WaitingForDeposit;
                    unit.Gather.WaitingForDepositTicks = 0;
                }

                return;

            case GatherResult.ResourceDepleted:

                if (!unit.Gather.IsFull
                    && GatherActions.TryRetargetResource(context, unit))
                {
                    return;
                }

                unit.Gather.Phase = GatherPhase.MovingToDeposit;

                if (!GatherActions.BeginMoveToDeposit(context, unit))
                {
                    unit.Gather.Phase = GatherPhase.WaitingForDeposit;
                    unit.Gather.WaitingForDepositTicks = 0;
                }

                return;

            case GatherResult.InvalidTarget:

                GatherActions.StopGathering(unit);

                return;
        }
    }
    private static void HandleMovingToDeposit(
    RuntimeContext context,
    ICommandQueue commandQueue,
    Unit unit)
    {   
        if (unit.Movement.NeedsRepath)
        {
            unit.Movement.NeedsRepath = false;

            if (!GatherActions.BeginMoveToDeposit(context, unit))
            {
                unit.Gather.Phase = GatherPhase.WaitingForDeposit;
                unit.Gather.WaitingForDepositTicks = 0;
            }

            return;
        }

        if (unit.Gather.DepositPosition is not GridPosition destination)
        {
            unit.Gather.Phase = GatherPhase.WaitingForDeposit;
            unit.Gather.WaitingForDepositTicks = 0;
            return;
        }

        if (!WorldQueries.HasReachedDestination(unit, destination))
        {
            if (unit.Movement.PathQueue.Count == 0
                && unit.Movement.CurrentStep == null)
            {
                unit.Gather.WaitingForDepositTicks++;

                if (unit.Gather.WaitingForDepositTicks >= StuckThreshold)
                {
                    unit.Gather.Phase = GatherPhase.WaitingForDeposit;
                    unit.Gather.WaitingForDepositTicks = 0;
                }
            }
            return;
        }

        unit.Gather.Phase = GatherPhase.Depositing;
    }
    private static void HandleWaitingForDeposit(
    RuntimeContext context,
    ICommandQueue commandQueue,
    Unit unit)
    {
        unit.Gather.WaitingForDepositTicks++;

        if (unit.Gather.WaitingForDepositTicks % RetryInterval == 0)
        {
            if (GatherActions.BeginMoveToDeposit(context, unit))
            {
                unit.Gather.Phase = GatherPhase.MovingToDeposit;
                unit.Gather.WaitingForDepositTicks = 0;
                return;
            }
        }

        if (unit.Gather.WaitingForDepositTicks >= MaxWaitTicks)
        {
            GatherActions.StopGathering(unit);
        }
    }
    private static void HandleDepositing(
    RuntimeContext context,
    ICommandQueue commandQueue,
    Unit unit)
    {
        var world = context.World;
        GatherActions.DepositInventory(world, unit);

       if (GatherActions.CanContinueGathering(world, unit))
        {
            unit.Gather.Phase = GatherPhase.MovingToResource;
            GatherActions.BeginMoveToResource(context, unit);
            return;
        }

        if (GatherActions.TryRetargetResource(context, unit))
        {
            unit.Gather.Phase = GatherPhase.MovingToResource;
            GatherActions.BeginMoveToResource(context, unit);
            return;
        }

        GatherActions.StopGathering(unit);
        unit.Gather.ClearInventory();
    }
}
