using RTSEngine.Core.AI.Lua;
using RTSEngine.Core.Actions;
using RTSEngine.Core.Entities.Definitions;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Players;
using RTSEngine.Core.State;
using RTSEngine.Tests.TestHelpers;

namespace RTSEngine.Tests.Lua;

public class LuaAiTests : IDisposable
{
    private readonly RuntimeContext _context;
    private readonly GameWorld _world;
    private readonly Player _player;
    private readonly string _testScriptsPath;

    public LuaAiTests()
    {
        _context = new RuntimeContext
        {
            World = TestWorldFactory.CreateWorldWithTwoPlayers(),
            UnitRepository = new UnitDefinitionRepository([
                TestDefinitionFactory.CreateVillager(),
                TestDefinitionFactory.CreateMilitia()
            ]),
            BuildingRepository = new BuildingDefinitionRepository([
                TestDefinitionFactory.CreateTownCenter(),
                TestDefinitionFactory.CreateBarracks(),
                TestDefinitionFactory.CreateHouse()
            ])
        };

        _world = _context.World;
        _player = _world.GetPlayerById(1)!;

        _testScriptsPath = Path.Combine(
            AppContext.BaseDirectory,
            "Data",
            "AI",
            "unit");

        Directory.CreateDirectory(_testScriptsPath);
    }

    [Fact]
    [Trait("Category", "Lua")]
    [Trait("Category", "Lua.Engine")]
    public void LuaEngine_LoadScript_ShouldReturnScript()
    {
        var scriptPath = Path.Combine(_testScriptsPath, "test.lua");
        File.WriteAllText(scriptPath, @"
            function onTick()
            end
        ");

        var engine = new LuaEngine(_testScriptsPath);
        var script = engine.LoadScript(_player, "test");

        Assert.NotNull(script);
        Assert.Equal(_player.Id, script.Player.Id);
    }

    [Fact]
    [Trait("Category", "Lua")]
    [Trait("Category", "Lua.Engine")]
    public void LuaEngine_LoadScript_ShouldReturnNull_WhenFileNotFound()
    {
        var engine = new LuaEngine(_testScriptsPath);
        var script = engine.LoadScript(_player, "nonexistent");

        Assert.Null(script);
    }

    [Fact]
    [Trait("Category", "Lua")]
    [Trait("Category", "Lua.Engine")]
    public void LuaEngine_Update_ShouldCallOnTick()
    {
        _player.Controller = PlayerControllerType.AI;

        var scriptPath = Path.Combine(_testScriptsPath, "test_ontick.lua");
        File.WriteAllText(scriptPath, @"
            tickCalled = false
            function onTick()
                tickCalled = true
            end
        ");

        var engine = new LuaEngine(_testScriptsPath);
        var script = engine.LoadScript(_player, "test_ontick");
        engine.Update(_context);

        Assert.NotNull(script);
        Assert.NotNull(script!.Globals);
        Assert.True(script.Globals!.Get("tickCalled").Boolean);
    }

    [Fact]
    [Trait("Category", "Lua")]
    [Trait("Category", "Lua.Api")]
    public void AiApi_GetWood_ShouldReturnPlayerResources()
    {
        _player.Economy.Add(ResourceType.Wood, 150);

        var api = new AiApi(_context, _player);

        Assert.Equal(150, api.GetWood());
    }

    [Fact]
    [Trait("Category", "Lua")]
    [Trait("Category", "Lua.Api")]
    public void AiApi_GetFood_ShouldReturnPlayerResources()
    {
        _player.Economy.Add(ResourceType.Food, 200);

        var api = new AiApi(_context, _player);

        Assert.Equal(200, api.GetFood());
    }

    [Fact]
    [Trait("Category", "Lua")]
    [Trait("Category", "Lua.Api")]
    public void AiApi_GetPopulation_ShouldReturnCurrentPop()
    {
        var api = new AiApi(_context, _player);

        Assert.Equal(0, api.GetPopulation());
    }

    [Fact]
    [Trait("Category", "Lua")]
    [Trait("Category", "Lua.Api")]
    public void AiApi_GetVillagerCount_ShouldCountVillagers()
    {
        var villager = UnitFactory.Create(
            TestDefinitionFactory.CreateVillager(),
            _player.Id,
            new GridPosition(5, 5));
        _world.AddEntity(villager);

        var api = new AiApi(_context, _player);

        Assert.Equal(1, api.GetVillagerCount());
    }

    [Fact]
    [Trait("Category", "Lua")]
    [Trait("Category", "Lua.Api")]
    public void AiApi_GetMilitiaCount_ShouldCountMilitia()
    {
        var militia = UnitFactory.Create(
            TestDefinitionFactory.CreateMilitia(),
            _player.Id,
            new GridPosition(5, 5));
        _world.AddEntity(militia);

        var api = new AiApi(_context, _player);

        Assert.Equal(1, api.GetMilitiaCount());
    }

    [Fact]
    [Trait("Category", "Lua")]
    [Trait("Category", "Lua.Api")]
    public void AiApi_GetIdleVillagerCount_ShouldCountIdle()
    {
        var villager = UnitFactory.Create(
            TestDefinitionFactory.CreateVillager(),
            _player.Id,
            new GridPosition(5, 5));
        villager.CurrentTask = UnitTask.Idle;
        _world.AddEntity(villager);

        var api = new AiApi(_context, _player);

        Assert.Equal(1, api.GetIdleVillagerCount());
    }

    [Fact]
    [Trait("Category", "Lua")]
    [Trait("Category", "Lua.Api")]
    public void AiApi_HasBuilding_ShouldReturnTrue_WhenBuildingExists()
    {
        var tc = BuildingFactory.Create(
            TestDefinitionFactory.CreateTownCenter(),
            _player.Id,
            new GridPosition(5, 5));
        tc.IsCompleted = true;
        _world.AddEntity(tc);

        var api = new AiApi(_context, _player);

        Assert.True(api.HasBuilding("town_center"));
    }

    [Fact]
    [Trait("Category", "Lua")]
    [Trait("Category", "Lua.Api")]
    public void AiApi_HasBuilding_ShouldReturnFalse_WhenNoBuilding()
    {
        var api = new AiApi(_context, _player);

        Assert.False(api.HasBuilding("town_center"));
    }

    [Fact]
    [Trait("Category", "Lua")]
    [Trait("Category", "Lua.Api")]
    public void AiApi_GetTownCenter_ShouldReturnId()
    {
        var tc = BuildingFactory.Create(
            TestDefinitionFactory.CreateTownCenter(),
            _player.Id,
            new GridPosition(5, 5));
        tc.IsCompleted = true;
        _world.AddEntity(tc);

        var api = new AiApi(_context, _player);

        Assert.Equal(tc.Id, api.GetTownCenter());
    }

    [Fact]
    [Trait("Category", "Lua")]
    [Trait("Category", "Lua.Api")]
    public void AiApi_GetTick_ShouldReturnWorldTick()
    {
        _world.AdvanceTick();
        _world.AdvanceTick();
        _world.AdvanceTick();

        var api = new AiApi(_context, _player);

        Assert.Equal(3, api.GetTick());
    }

    [Fact]
    [Trait("Category", "Lua")]
    [Trait("Category", "Lua.Api")]
    public void AiApi_GetUnitX_ShouldReturnPosition()
    {
        var villager = UnitFactory.Create(
            TestDefinitionFactory.CreateVillager(),
            _player.Id,
            new GridPosition(3, 7));
        _world.AddEntity(villager);

        var api = new AiApi(_context, _player);

        Assert.Equal(3, api.GetUnitX(villager.Id));
        Assert.Equal(7, api.GetUnitY(villager.Id));
    }

    [Fact]
    [Trait("Category", "Lua")]
    [Trait("Category", "Lua.Api")]
    public void AiApi_GetUnitTask_ShouldReturnTask()
    {
        var villager = UnitFactory.Create(
            TestDefinitionFactory.CreateVillager(),
            _player.Id,
            new GridPosition(5, 5));
        villager.CurrentTask = UnitTask.Gathering;
        _world.AddEntity(villager);

        var api = new AiApi(_context, _player);

        Assert.Equal("Gathering", api.GetUnitTask(villager.Id));
    }

    [Fact]
    [Trait("Category", "Lua")]
    [Trait("Category", "Lua.Api")]
    public void AiApi_Move_ShouldAddCommand()
    {
        var villager = UnitFactory.Create(
            TestDefinitionFactory.CreateVillager(),
            _player.Id,
            new GridPosition(5, 5));
        _world.AddEntity(villager);

        var api = new AiApi(_context, _player);

        Assert.True(api.Move(villager.Id, 8, 8));
        Assert.Single(_world.PendingCommands);
    }

    [Fact]
    [Trait("Category", "Lua")]
    [Trait("Category", "Lua.Api")]
    public void AiApi_Move_ShouldReturnFalse_WhenInvalidUnit()
    {
        var api = new AiApi(_context, _player);

        Assert.False(api.Move(999, 8, 8));
    }

    [Fact]
    [Trait("Category", "Lua")]
    [Trait("Category", "Lua.Api")]
    public void AiApi_Stop_ShouldSetIdle()
    {
        var villager = UnitFactory.Create(
            TestDefinitionFactory.CreateVillager(),
            _player.Id,
            new GridPosition(5, 5));
        villager.CurrentTask = UnitTask.Moving;
        _world.AddEntity(villager);

        var api = new AiApi(_context, _player);

        Assert.True(api.Stop(villager.Id));
        Assert.Equal(UnitTask.Idle, villager.CurrentTask);
    }

    [Fact]
    [Trait("Category", "Lua")]
    [Trait("Category", "Lua.Api")]
    public void AiApi_TrainVillager_ShouldReturnTrue_WhenValid()
    {
        _player.Economy.Add(ResourceType.Food, 100);
        PopulationActions.IncreaseCap(_player, 10);

        var tc = BuildingFactory.Create(
            TestDefinitionFactory.CreateTownCenter(),
            _player.Id,
            new GridPosition(5, 5));
        tc.IsCompleted = true;
        tc.Production.SpawnPoint = new GridPosition(7, 5);
        _world.AddEntity(tc);

        var api = new AiApi(_context, _player);

        Assert.True(api.TrainVillager(tc.Id));
    }

    [Fact]
    [Trait("Category", "Lua")]
    [Trait("Category", "Lua.Api")]
    public void AiApi_TrainVillager_ShouldReturnFalse_WhenNoFood()
    {
        var tc = BuildingFactory.Create(
            TestDefinitionFactory.CreateTownCenter(),
            _player.Id,
            new GridPosition(5, 5));
        tc.IsCompleted = true;
        _world.AddEntity(tc);

        var api = new AiApi(_context, _player);

        Assert.False(api.TrainVillager(tc.Id));
    }

    public void Dispose()
    {
        if (Directory.Exists(_testScriptsPath))
        {
            foreach (var file in Directory.GetFiles(_testScriptsPath, "test*.lua"))
            {
                File.Delete(file);
            }
        }
    }
}
