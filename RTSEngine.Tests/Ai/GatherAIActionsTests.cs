using RTSEngine.Core.AI.Actions;
using RTSEngine.Core.Commands;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Tests.TestHelpers;
using RTSEngine.Core.Entities.Resources;
namespace RTSEngine.Tests.AI;

public class GatherAIActionsTests
{
    [Fact]
    [Trait("Category", "AI")]
    [Trait("Category", "AI.Gather")]
    public void AssignGatherTask_ShouldQueueGatherCommand()
    {
        // Arrange
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();

        var player = world.GetPlayerById(1);
        Assert.NotNull(player);

        var villager = UnitFactory.Create(
            TestDefinitionFactory.CreateVillager(),
            player.Id,
            new GridPosition(5, 5));

        world.Entities.Add(villager, player);

        var tree = new Tree(new GridPosition(10, 5));
        world.Entities.Add(tree);

        // Act
        var queue = new CommandQueue();
        GatherAIActions.AssignGatherTask(
            queue,
            villager,
            tree);

        // Assert
        var command = Assert.Single(queue.Pending);

        var gatherCommand = Assert.IsType<GatherCommand>(command);

        Assert.Single(gatherCommand.UnitIds);
        Assert.Equal(villager.Id, gatherCommand.UnitIds[0]);
        Assert.Equal(tree.Id, gatherCommand.ResourceId);
    }

    [Fact]
    [Trait("Category", "AI")]
    [Trait("Category", "AI.Gather")]
    public void AssignGatherTask_ShouldAppendCommandToExistingQueue()
    {
        // Arrange
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();

        var player = world.GetPlayerById(1);
        Assert.NotNull(player);

        var villager = UnitFactory.Create(
            TestDefinitionFactory.CreateVillager(),
            player.Id,
            new GridPosition(5, 5));

        world.Entities.Add(villager, player);

        var tree = new Tree(new GridPosition(10, 5));
        world.Entities.Add(tree);

        var queue = new CommandQueue();
        queue.Enqueue(new GatherCommand
        {
            UnitIds = [999],
            ResourceId = 999
        });

        // Act
        GatherAIActions.AssignGatherTask(
            queue,
            villager,
            tree);

        // Assert
        Assert.Equal(2, queue.Pending.Count);

        var gatherCommand = Assert.IsType<GatherCommand>(
            queue.Pending.Last());

        Assert.Equal(villager.Id, gatherCommand.UnitIds[0]);
        Assert.Equal(tree.Id, gatherCommand.ResourceId);
    }
}