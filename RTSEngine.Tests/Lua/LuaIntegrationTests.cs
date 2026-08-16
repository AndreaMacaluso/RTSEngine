using MoonSharp.Interpreter;
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

public class LuaIntegrationTests : IDisposable
{
    private readonly RuntimeContext _context;
    private readonly GameWorld _world;
    private readonly Player _player;
    private readonly string _testScriptsPath;

    public LuaIntegrationTests()
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
            "integration");

        Directory.CreateDirectory(_testScriptsPath);
    }

    [Fact]
    [Trait("Category", "Lua")]
    [Trait("Category", "Lua.Integration")]
    public void LuaScript_ShouldCallGetWood()
    {
        _player.Economy.Add(ResourceType.Wood, 250);

        var scriptPath = Path.Combine(_testScriptsPath, "test_getwood.lua");
        File.WriteAllText(scriptPath, @"
            woodValue = 0
            function onTick()
                woodValue = ai.getWood()
            end
        ");

        var engine = new LuaEngine(_testScriptsPath);
        var script = engine.LoadScript(_player, "test_getwood");
        Assert.NotNull(script);

        script.Update(_context);

        var result = script.Globals?.Get("woodValue");
        Assert.NotNull(result);
        Assert.Equal(250.0, result.Number);
    }

    [Fact]
    [Trait("Category", "Lua")]
    [Trait("Category", "Lua.Integration")]
    public void LuaScript_ShouldCallGetVillagerCount()
    {
        var villager = UnitFactory.Create(
            TestDefinitionFactory.CreateVillager(),
            _player.Id,
            new GridPosition(5, 5));
        _world.AddEntity(villager);

        var scriptPath = Path.Combine(_testScriptsPath, "test_getvillagers.lua");
        File.WriteAllText(scriptPath, @"
            villagerCount = 0
            function onTick()
                villagerCount = ai.getVillagerCount()
            end
        ");

        var engine = new LuaEngine(_testScriptsPath);
        var script = engine.LoadScript(_player, "test_getvillagers");
        Assert.NotNull(script);

        script.Update(_context);

        var result = script.Globals?.Get("villagerCount");
        Assert.NotNull(result);
        Assert.Equal(1.0, result.Number);
    }

    [Fact]
    [Trait("Category", "Lua")]
    [Trait("Category", "Lua.Integration")]
    public void LuaScript_ShouldCallHasBuilding()
    {
        var tc = BuildingFactory.Create(
            TestDefinitionFactory.CreateTownCenter(),
            _player.Id,
            new GridPosition(5, 5));
        tc.IsCompleted = true;
        _world.AddEntity(tc);

        var scriptPath = Path.Combine(_testScriptsPath, "test_hasbuilding.lua");
        File.WriteAllText(scriptPath, @"
            hasTC = false
            function onTick()
                hasTC = ai.hasBuilding('town_center')
            end
        ");

        var engine = new LuaEngine(_testScriptsPath);
        var script = engine.LoadScript(_player, "test_hasbuilding");
        Assert.NotNull(script);

        script.Update(_context);

        var result = script.Globals?.Get("hasTC");
        Assert.NotNull(result);
        Assert.True(result.Boolean);
    }

    [Fact]
    [Trait("Category", "Lua")]
    [Trait("Category", "Lua.Integration")]
    public void LuaScript_ShouldCallMove()
    {
        var villager = UnitFactory.Create(
            TestDefinitionFactory.CreateVillager(),
            _player.Id,
            new GridPosition(5, 5));
        _world.AddEntity(villager);

        var scriptPath = Path.Combine(_testScriptsPath, "test_move.lua");
        File.WriteAllText(scriptPath, @"
            moveResult = false
            function onTick()
                moveResult = ai.move(" + villager.Id + @", 8, 8)
            end
        ");

        var engine = new LuaEngine(_testScriptsPath);
        var script = engine.LoadScript(_player, "test_move");
        Assert.NotNull(script);

        script.Update(_context);

        Assert.Single(_world.PendingCommands);
    }

    [Fact]
    [Trait("Category", "Lua")]
    [Trait("Category", "Lua.Integration")]
    public void LuaScript_ShouldCallStop()
    {
        var villager = UnitFactory.Create(
            TestDefinitionFactory.CreateVillager(),
            _player.Id,
            new GridPosition(5, 5));
        villager.CurrentTask = UnitTask.Moving;
        _world.AddEntity(villager);

        var scriptPath = Path.Combine(_testScriptsPath, "test_stop.lua");
        File.WriteAllText(scriptPath, @"
            stopResult = false
            function onTick()
                stopResult = ai.stop(" + villager.Id + @")
            end
        ");

        var engine = new LuaEngine(_testScriptsPath);
        var script = engine.LoadScript(_player, "test_stop");
        Assert.NotNull(script);

        script.Update(_context);

        Assert.Equal(UnitTask.Idle, villager.CurrentTask);
    }

    [Fact]
    [Trait("Category", "Lua")]
    [Trait("Category", "Lua.Integration")]
    public void LuaScript_ShouldAccessMultipleApiMethods()
    {
        _player.Economy.Add(ResourceType.Wood, 100);
        _player.Economy.Add(ResourceType.Food, 200);

        var villager = UnitFactory.Create(
            TestDefinitionFactory.CreateVillager(),
            _player.Id,
            new GridPosition(5, 5));
        _world.AddEntity(villager);

        var scriptPath = Path.Combine(_testScriptsPath, "test_multi.lua");
        File.WriteAllText(scriptPath, @"
            results = {}
            function onTick()
                results.wood = ai.getWood()
                results.food = ai.getFood()
                results.villagers = ai.getVillagerCount()
                results.idle = ai.getIdleVillagerCount()
                results.tick = ai.getTick()
            end
        ");

        var engine = new LuaEngine(_testScriptsPath);
        var script = engine.LoadScript(_player, "test_multi");
        Assert.NotNull(script);

        script.Update(_context);

        var results = script.Globals?.Get("results")?.Table;
        Assert.NotNull(results);
        Assert.Equal(100.0, results.Get("wood").Number);
        Assert.Equal(200.0, results.Get("food").Number);
        Assert.Equal(1.0, results.Get("villagers").Number);
        Assert.Equal(1.0, results.Get("idle").Number);
    }

    [Fact]
    [Trait("Category", "Lua")]
    [Trait("Category", "Lua.Integration")]
    public void LuaScript_ShouldExecuteConditionalLogic()
    {
        _player.Economy.Add(ResourceType.Food, 60);

        var tc = BuildingFactory.Create(
            TestDefinitionFactory.CreateTownCenter(),
            _player.Id,
            new GridPosition(5, 5));
        tc.IsCompleted = true;
        tc.Production.SpawnPoint = new GridPosition(7, 5);
        _world.AddEntity(tc);

        PopulationActions.IncreaseCap(_player, 10);

        var scriptPath = Path.Combine(_testScriptsPath, "test_conditional.lua");
        File.WriteAllText(scriptPath, @"
            trained = false
            function onTick()
                if ai.getFood() >= 50 and ai.hasBuilding('town_center') then
                    ai.trainVillager(ai.getTownCenter())
                    trained = true
                end
            end
        ");

        var engine = new LuaEngine(_testScriptsPath);
        var script = engine.LoadScript(_player, "test_conditional");
        Assert.NotNull(script);

        script.Update(_context);

        var result = script.Globals?.Get("trained");
        Assert.NotNull(result);
        Assert.True(result.Boolean);
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
