using RTSEngine.Core.State;
using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Entities.States;
using RTSEngine.Core.Actions;
using RTSEngine.Core.Helpers;

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
        if (!WorldQueries.HasReachedDestination(unit))
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

        GatherActions.BeginMoveToDeposit(
            world,
            unit);
    }
    private static void HandleMovingToDeposit(
    GameWorld world,
    Unit unit)
    {
        if (!WorldQueries.HasReachedDestination(unit))
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
        }
    }
}