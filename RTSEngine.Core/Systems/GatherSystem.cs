using RTSEngine.Core.State;
using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Entities.States;
using RTSEngine.Core.Actions;
using RTSEngine.Core.Diagnostics;
using RTSEngine.Core.Helpers;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Actions;

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
    private static void HandleMovingToResource(
    GameWorld world,
    Unit unit)
    {   

        if (!GatherActions.CanContinueGathering(world, unit))
        {
            GatherActions.StopGathering(unit);
            return;
        }

        if (unit.Gather.TargetResourceId is not int resourceId)
        {
            return;
        }

        var resource = world.GetResourceById(resourceId);

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
    GameWorld world,
    Unit unit)
    {
        switch (GatherActions.GatherOneTick(world, unit))
        {
            case GatherResult.ContinueGathering:
                return;

            case GatherResult.InventoryFull:

                unit.Gather.Phase = GatherPhase.MovingToDeposit;

                if (!GatherActions.BeginMoveToDeposit(world, unit))
                {
                    GatherActions.StopGathering(unit);
                }

                return;

            case GatherResult.ResourceDepleted:

                unit.Gather.Phase = GatherPhase.MovingToDeposit;

                if (!GatherActions.BeginMoveToDeposit(world, unit))
                {
                    GatherActions.StopGathering(unit);
                }

                return;

            case GatherResult.InvalidTarget:

                GatherActions.StopGathering(unit);

                return;
        }
    }
    private static void HandleMovingToDeposit(
    GameWorld world,
    Unit unit)
    {   
        if (unit.Movement.NeedsRepath)
        {
            unit.Movement.NeedsRepath = false;

            if (!GatherActions.BeginMoveToDeposit(world, unit))
            {
                GatherActions.StopGathering(unit);
            }

            return;
        }
        if (unit.Gather.DepositPosition is not GridPosition destination)
        {
            return;
        }

        if (!WorldQueries.HasReachedDestination(unit, destination))
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