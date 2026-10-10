using RTSEngine.Core.Commands;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Map.Visibility;
using RTSEngine.Core.Settings;
using RTSEngine.Core.State;
using RTSEngine.Core.Systems;
using RTSEngine.Tests.TestHelpers;
using Xunit;

namespace RTSEngine.Tests.Visibility;

/// <summary>
/// An explicit move order is never gated by the fog. Right-clicking into the
/// black is the core of an RTS, so the gate must stay out of the command path.
/// </summary>
[Trait("Category", "Visibility")]
public class MovementFogTests
{
    [Fact]
    public void MoveCommand_OnUnseenTile_IsAcceptedAndDoesNotScout()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers(
            width: 40, height: 40, visibility: MapVisibility.Normal);

        var unit = UnitFactory.Create(
            TestDefinitionFactory.CreateVillager(), 1, new GridPosition(20, 20));
        world.Entities.Add(unit, world.GetPlayerById(1)!);

        var context = SimulationTestHelper.CreateContext(world);
        VisibilitySystem.Update(context);

        // own tile is in sight, the destination has never been seen
        var origin = new GridPosition(20, 20);
        var destination = new GridPosition(0, 39);
        Assert.Equal(TileVisibility.Visible, world.Fog.Get(1, origin));
        Assert.Equal(TileVisibility.Hidden, world.Fog.Get(1, destination));

        context.CommandQueue.Enqueue(new MoveCommand
        {
            UnitIds = [unit.Id],
            PlayerId = 1,
            Target = destination
        });

        CommandSystem.Update(context);

        // the order is accepted: the gate does not live in the command path
        Assert.Equal(destination, unit.Movement.Destination);

        // and issuing it does not scout: the tile is still black
        Assert.Equal(TileVisibility.Hidden, world.Fog.Get(1, destination));
    }
}
