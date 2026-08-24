using RTSEngine.Core.AI;
using RTSEngine.Core.AI.Brains;
using RTSEngine.Core.Commands;
using RTSEngine.Core.Entities.Buildings;
using RTSEngine.Core.Entities.Definitions;
using RTSEngine.Core.Entities.Resources;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Players;
using RTSEngine.Core.State;
using RTSEngine.Core.Systems;
using RTSEngine.Tests.TestHelpers;

namespace RTSEngine.Tests.AI.Brains;

public class GatherBrainTests
{
    private readonly RuntimeContext _context;
    private readonly GameWorld _world;
    private readonly Player _player;

    public GatherBrainTests()
    {
        _context = new RuntimeContext
        {
            World = TestWorldFactory.CreateWorldWithTwoPlayers(),
            UnitRepository = new UnitDefinitionRepository([
                TestDefinitionFactory.CreateVillager()
            ]),
            BuildingRepository = new BuildingDefinitionRepository([
                TestDefinitionFactory.CreateTownCenter()
            ]),
            CommandQueue = new CommandQueue()
        };

        _world = _context.World;
        _player = _world.GetPlayerById(1)!;
    }

    private Building CreateCompletedBuilding(BuildingDefinition def, int ownerId, GridPosition pos)
    {
        var building = BuildingFactory.Create(def, ownerId, pos);
        building.IsCompleted = true;
        return building;
    }

    [Fact]
    [Trait("Category", "AI")]
    [Trait("Category", "Brain")]
    public void ShouldAssignVillagerToResource()
    {
        CreateCompletedBuilding(
            TestDefinitionFactory.CreateTownCenter(),
            _player.Id,
            new GridPosition(0, 0));

        var villager = UnitFactory.Create(
            TestDefinitionFactory.CreateVillager(),
            _player.Id,
            new GridPosition(5, 5));
        _world.Entities.Add(villager, _player);

        _world.Entities.Add(new Tree(new GridPosition(10, 5)));

        var brain = new GatherBrain();
        brain.Execute(_context, _player);
        CommandSystem.Update(_context);

        Assert.Equal(UnitTask.Gathering, villager.CurrentTask);
        Assert.NotNull(villager.Gather.TargetResourceId);
    }

    [Fact]
    [Trait("Category", "AI")]
    [Trait("Category", "Brain")]
    public void ShouldNotAssignBusyVillager()
    {
        CreateCompletedBuilding(
            TestDefinitionFactory.CreateTownCenter(),
            _player.Id,
            new GridPosition(0, 0));

        var villager = UnitFactory.Create(
            TestDefinitionFactory.CreateVillager(),
            _player.Id,
            new GridPosition(5, 5));
        villager.CurrentTask = UnitTask.Gathering;
        _world.Entities.Add(villager, _player);

        _world.Entities.Add(new Tree(new GridPosition(10, 5)));

        var brain = new GatherBrain();
        brain.Execute(_context, _player);

        Assert.Equal(UnitTask.Gathering, villager.CurrentTask);
    }

    [Fact]
    [Trait("Category", "AI")]
    [Trait("Category", "Brain")]
    public void ShouldDoNothing_WhenNoIdleVillagers()
    {
        CreateCompletedBuilding(
            TestDefinitionFactory.CreateTownCenter(),
            _player.Id,
            new GridPosition(0, 0));

        var villager = UnitFactory.Create(
            TestDefinitionFactory.CreateVillager(),
            _player.Id,
            new GridPosition(5, 5));
        villager.CurrentTask = UnitTask.Gathering;
        _world.Entities.Add(villager, _player);

        var brain = new GatherBrain();
        brain.Execute(_context, _player);
    }

    [Fact]
    [Trait("Category", "AI")]
    [Trait("Category", "Brain")]
    public void ShouldSkipResource_WhenTargetReached()
    {
        CreateCompletedBuilding(
            TestDefinitionFactory.CreateTownCenter(),
            _player.Id,
            new GridPosition(0, 0));

        var villager = UnitFactory.Create(
            TestDefinitionFactory.CreateVillager(),
            _player.Id,
            new GridPosition(5, 5));
        _world.Entities.Add(villager, _player);

        _world.Entities.Add(new Tree(new GridPosition(10, 5)));
        _player.Economy.Add(ResourceType.Wood, 500);

        var brain = new GatherBrain();
        brain.Execute(_context, _player);

        Assert.Equal(UnitTask.Idle, villager.CurrentTask);
    }

    [Fact]
    [Trait("Category", "AI")]
    [Trait("Category", "Brain")]
    public void ShouldAssignMultipleVillagers()
    {
        CreateCompletedBuilding(
            TestDefinitionFactory.CreateTownCenter(),
            _player.Id,
            new GridPosition(0, 0));

        var villager1 = UnitFactory.Create(
            TestDefinitionFactory.CreateVillager(),
            _player.Id,
            new GridPosition(5, 5));
        var villager2 = UnitFactory.Create(
            TestDefinitionFactory.CreateVillager(),
            _player.Id,
            new GridPosition(6, 5));
        _world.Entities.Add(villager1, _player);
        _world.Entities.Add(villager2, _player);

        _world.Entities.Add(new Tree(new GridPosition(10, 5)));

        var brain = new GatherBrain();
        brain.Execute(_context, _player);
        CommandSystem.Update(_context);

        Assert.Equal(UnitTask.Gathering, villager1.CurrentTask);
        Assert.Equal(UnitTask.Gathering, villager2.CurrentTask);
        Assert.NotNull(villager1.Gather.TargetResourceId);
        Assert.NotNull(villager2.Gather.TargetResourceId);
    }
}
