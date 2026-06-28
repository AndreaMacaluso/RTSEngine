using RTSEngine.Core.Commands;
using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Map;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.State;
using RTSEngine.Core.Systems;
using RTSEngine.Tests.TestHelpers;
using RTSEngine.Core.Entities.Runtime;

namespace RTSEngine.Tests.Systems;

public class CommandSystemTests
{
    [Fact]
    public void MoveCommand_ShouldAssignPathQueue()
    {
        var world = TestWorldFactory.CreateWorld();
        
        var villagerDefinition = new UnitDefinition
        {
            Id = "villager",
            Name = "Villager",
            MaxHealth = 50,
            MovementSpeed = 1f
        };

        var unit = UnitFactory.Create(
            villagerDefinition,
            1,
            new GridPosition(1, 1));
      

        world.AddEntity(unit);

        var command = new MoveCommand
        {
            UnitIds = [unit.Id],
            Target = new GridPosition(5, 5)
        };

        world.AddCommand(command);

        CommandSystem.Update(world);

        Assert.NotEmpty(unit.Movement.PathQueue);
    }
}