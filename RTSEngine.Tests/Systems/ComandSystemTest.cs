using RTSEngine.Core.Commands;
using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Map;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.State;
using RTSEngine.Core.Systems;
using RTSEngine.Tests.TestHelpers;

namespace RTSEngine.Tests.Systems;

public class CommandSystemTests
{
    [Fact]
    public void MoveCommand_ShouldAssignPathQueue()
    {
        var world = TestWorldFactory.CreateWorld();

        var unit = new Villager(
            ownerId: 1,
            position: new GridPosition(1, 1)
            );

        world.AddEntity(unit);

        var command = new MoveCommand
        {
            UnitIds = [unit.Id],
            Target = new GridPosition(5, 5)
        };

        world.AddCommand(command);

        CommandSystem.Update(world);

        Assert.NotEmpty(unit.PathQueue);
    }
}