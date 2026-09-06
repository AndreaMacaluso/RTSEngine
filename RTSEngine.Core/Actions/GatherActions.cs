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
using RTSEngine.Core.Entities.Runtime;

namespace RTSEngine.Core.Actions;

public static class GatherActions
{
    public static bool BeginMoveToResource(
    RuntimeContext context,
    Unit unit)
    {
        var resource = GetTargetResource(context.World, unit);

        if (resource == null)
        {
            return false;
        }

        unit.Gather.DepositPosition = null;

        GridPosition? target =
            WorldQueries.FindClosestAdjacentWalkableTile(
                context.World,
                unit.Position,
                resource.Position);

        if (target is not GridPosition destination)
        {
            return false;
        }

        CommandSystem.AssignMoveTarget(unit, destination, context);

        return true;
    }

    public static bool BeginMoveToDeposit(
    RuntimeContext context,
    Unit unit)
    {

        DebugSession.Log.Debug(
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
            context.World,
            context.World.GetPlayerById(unit.OwnerId)!,
            unit.Position,
            resourceType);
        DebugSession.Log.Debug(
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
            context.World,
            unit.Position,
            deposit.Position);

        if (target is not GridPosition destination)
        {
            return false;
        }

        CommandSystem.AssignMoveTarget(unit, destination, context);
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

    // NOTE: DepositInventory returns void. If the player or resource type is
    // null, the inventory is never cleared and resources are never transferred.
    // The caller (HandleDepositing) has no way to detect this failure.
    // Consider returning bool for error handling.
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
        player.Economy.Add(gatheredResource,unit.Gather.CurrentLoad);
        unit.Gather.ClearInventory();
    }

    public static bool CanContinueGathering(
        GameWorld world,
        Unit unit)
    {
        var resource = GetTargetResource(world, unit);

        return resource != null && !resource.IsDepleted;
    }

    public static bool TryRetargetResource(
    RuntimeContext context,
    Unit unit)
    {
        GameWorld world = context.World;

        if (unit.Gather.CarriedResource is not ResourceType resourceType)
        {
            return false;
        }

        var deposit = WorldQueries.FindClosestDeposit(
            world,
            world.GetPlayerById(unit.OwnerId)!,
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
        unit.CurrentTask = EntityState.Gathering;
        unit.Gather.Phase = GatherPhase.MovingToResource;
        unit.Gather.CarriedResource = resourceType;
        CommandSystem.AssignMoveTarget(unit, target.Value, context);

        return true;
    }

    public static void StopGathering(Unit unit)
    {
        unit.Gather.TargetResourceId = null;
        unit.Gather.DepositPosition = null;
        unit.Gather.Phase = GatherPhase.None;
        unit.CurrentTask = EntityState.Idle;
    }

    private static ResourceNode? GetTargetResource(
    GameWorld world,
    Unit unit)
    {
        if (unit.Gather.TargetResourceId is not int resourceId)
        {
            return null;
        }

        return world.Entities.GetResourceById(resourceId);
    }

}