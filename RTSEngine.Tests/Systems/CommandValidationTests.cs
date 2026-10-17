using RTSEngine.Core.State;
using RTSEngine.Core.Systems;
using RTSEngine.Core.Commands;
using RTSEngine.Core.Entities.Buildings;
using RTSEngine.Core.Entities.Definitions;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Helpers;
using RTSEngine.Tests.TestHelpers;
using RTSEngine.Core.Settings;
using RTSEngine.Core.Systems.Pathfinding;
using RTSEngine.Core.Events;

namespace RTSEngine.Tests.Systems;

public class CommandValidationTests
{
    private RuntimeContext CreateContext(GameWorld world, List<GameEvent> captured)
    {
        var context = new RuntimeContext
        {
            World = world,
            UnitRepository = new UnitDefinitionRepository([]),
            BuildingRepository = new BuildingDefinitionRepository([]),
            CommandQueue = new CommandQueue(),
            PathFinder = new AStarPathFinder(new GroundMovementFilter()),
            Settings = new GameSettings(),
            Events = new EventBus()
        };

        context.Events.Subscribe(captured.Add);

        return context;
    }

    private static void AssertRejected(
        List<GameEvent> captured,
        string expectedPayload)
    {
        var rejected = Assert.Single(captured);
        Assert.Equal(EventType.CommandRejected, rejected.Type);
        Assert.Equal(expectedPayload, rejected.Payload);
    }

    [Fact]
    [Trait("Category", "CommandValidation")]
    public void EmptyUnitList_PublishesEmptyReason()
    {
        var captured = new List<GameEvent>();
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var context = CreateContext(world, captured);

        context.CommandQueue.Enqueue(new MoveCommand
        {
            UnitIds = [],
            PlayerId = 1,
            Target = new GridPosition(5, 5)
        });

        CommandSystem.Update(context);
        context.Events.Flush();

        AssertRejected(captured, "Empty:MoveCommand");
    }

    [Fact]
    [Trait("Category", "CommandValidation")]
    public void TooManyUnits_PublishesTooManyUnitsReason()
    {
        var captured = new List<GameEvent>();
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var context = CreateContext(world, captured);

        context.CommandQueue.Enqueue(new MoveCommand
        {
            UnitIds = Enumerable.Repeat(1, 257).ToList(),
            PlayerId = 1,
            Target = new GridPosition(5, 5)
        });

        CommandSystem.Update(context);
        context.Events.Flush();

        AssertRejected(captured, "TooManyUnits:MoveCommand");
    }

    [Fact]
    [Trait("Category", "CommandValidation")]
    public void AtUnitLimit_PassesValidation_NoRejectionEvent()
    {
        var captured = new List<GameEvent>();
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var context = CreateContext(world, captured);

        context.CommandQueue.Enqueue(new MoveCommand
        {
            UnitIds = Enumerable.Repeat(1, 256).ToList(),
            PlayerId = 1,
            Target = new GridPosition(5, 5)
        });

        CommandSystem.Update(context);
        context.Events.Flush();

        Assert.Empty(captured);
    }

    [Fact]
    [Trait("Category", "CommandValidation")]
    public void AttackEntity_WithoutTarget_PublishesMissingTargetReason()
    {
        var captured = new List<GameEvent>();
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var context = CreateContext(world, captured);

        context.CommandQueue.Enqueue(new AttackCommand
        {
            UnitIds = [1],
            PlayerId = 1,
            Mode = AttackMode.Entity
        });

        CommandSystem.Update(context);
        context.Events.Flush();

        AssertRejected(captured, "MissingTarget:AttackCommand");
    }

    [Fact]
    [Trait("Category", "CommandValidation")]
    public void Gather_WithoutResource_PublishesMissingTargetReason()
    {
        var captured = new List<GameEvent>();
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var context = CreateContext(world, captured);

        context.CommandQueue.Enqueue(new GatherCommand
        {
            UnitIds = [1],
            PlayerId = 1,
            ResourceId = 0
        });

        CommandSystem.Update(context);
        context.Events.Flush();

        AssertRejected(captured, "MissingTarget:GatherCommand");
    }

    [Fact]
    [Trait("Category", "CommandValidation")]
    public void Build_WithoutBuilding_PublishesMissingTargetReason()
    {
        var captured = new List<GameEvent>();
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var context = CreateContext(world, captured);

        context.CommandQueue.Enqueue(new BuildCommand
        {
            UnitIds = [1],
            PlayerId = 1,
            BuildingId = 0
        });

        CommandSystem.Update(context);
        context.Events.Flush();

        AssertRejected(captured, "MissingTarget:BuildCommand");
    }

    [Fact]
    [Trait("Category", "CommandValidation")]
    public void ProductionQueueUnit_WithoutProductId_PublishesMissingTargetReason()
    {
        var captured = new List<GameEvent>();
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var context = CreateContext(world, captured);

        context.CommandQueue.Enqueue(new ProductionCommand
        {
            PlayerId = 1,
            BuildingId = 1,
            Action = ProductionActionType.QueueUnit
        });

        CommandSystem.Update(context);
        context.Events.Flush();

        AssertRejected(captured, "MissingTarget:ProductionCommand");
    }

    [Fact]
    [Trait("Category", "CommandValidation")]
    public void UnknownCommand_PublishesUnknownCommandReason()
    {
        var captured = new List<GameEvent>();
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var context = CreateContext(world, captured);

        context.CommandQueue.Enqueue(new UnknownTestCommand());

        CommandSystem.Update(context);
        context.Events.Flush();

        AssertRejected(captured, "UnknownCommand:UnknownTestCommand");
    }

    [Fact]
    [Trait("Category", "CommandValidation")]
    public void ForeignUnit_PublishesNotOwnerReason()
    {
        var captured = new List<GameEvent>();
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var context = CreateContext(world, captured);
        var player2 = world.GetPlayerById(2)!;

        var enemyUnit = UnitFactory.Create(
            TestDefinitionFactory.CreateMilitiaWithCombatStats(),
            2,
            new GridPosition(5, 5));
        world.Entities.Add(enemyUnit, player2);

        context.CommandQueue.Enqueue(new MoveCommand
        {
            UnitIds = [enemyUnit.Id],
            PlayerId = 1,
            Target = new GridPosition(6, 5)
        });

        CommandSystem.Update(context);
        context.Events.Flush();

        AssertRejected(captured, "NotOwner:MoveCommand");
    }

    [Fact]
    [Trait("Category", "CommandValidation")]
    public void ForeignBuilding_Production_PublishesNotOwnerReason()
    {
        var captured = new List<GameEvent>();
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var context = CreateContext(world, captured);
        var player1 = world.GetPlayerById(1)!;

        var building = BuildingFactory.Create(
            TestDefinitionFactory.CreateTownCenter(),
            1,
            new GridPosition(5, 5));
        building.IsCompleted = true;
        building.Health.CurrentHealth = building.Definition.MaxHealth;
        world.Entities.Add(building, player1);

        context.CommandQueue.Enqueue(new ProductionCommand
        {
            PlayerId = 2,
            BuildingId = building.Id,
            Action = ProductionActionType.SetRallyPoint,
            Target = new GridPosition(8, 8)
        });

        CommandSystem.Update(context);
        context.Events.Flush();

        AssertRejected(captured, "NotOwner:ProductionCommand");
    }

    [Fact]
    [Trait("Category", "CommandValidation")]
    public void MaxUnitsPerCommand_IsAtLeastPopulationCap()
    {
        Assert.True(
            new EngineSettings().MaxUnitsPerCommand
                >= new GameSettings().PopulationCap);
    }

    private sealed class UnknownTestCommand : ICommand;
}
