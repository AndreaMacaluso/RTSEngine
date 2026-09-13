using RTSEngine.Core.Commands;
using RTSEngine.Core.Entities.Definitions;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Players;
using RTSEngine.Core.Settings;
using RTSEngine.Core.State;
using RTSEngine.Core.Systems.Pathfinding;
using RTSEngine.Core.Triggers;
using RTSEngine.Tests.TestHelpers;
using System.Text.Json;

namespace RTSEngine.Tests.Triggers;

public class TriggerSystemTests
{
    private readonly RuntimeContext _context;
    private readonly GameWorld _world;
    private readonly Player _player;
    private readonly UnitDefinition _villagerDefinition;

    public TriggerSystemTests()
    {
        _world = TestWorldFactory.CreateWorldWithTwoPlayers(width: 40, height: 40);
        _player = _world.GetPlayerById(1)!;
        _villagerDefinition = TestDefinitionFactory.CreateVillager();

        var unitRepository = new UnitDefinitionRepository(new[] { _villagerDefinition });
        var buildingRepository = new BuildingDefinitionRepository(new BuildingDefinition[0]);

        _context = new RuntimeContext
        {
            World = _world,
            UnitRepository = unitRepository,
            BuildingRepository = buildingRepository,
            CommandQueue = new CommandQueue(),
            PathFinder = new AStarPathFinder(new GroundMovementFilter()),
            Settings = new GameSettings()
        };
    }

    private void AddUnits(int count)
    {
        for (int i = 0; i < count; i++)
        {
            var unit = UnitFactory.Create(_villagerDefinition, _player.Id, new GridPosition(i, 0));
            _world.Entities.Add(unit, _player);
        }
    }

    [Fact]
    [Trait("Category", "Triggers")]
    public void OwnObjectsCondition_ShouldReturnTrue_WhenEnoughUnits()
    {
        AddUnits(10);
        var condition = new OwnObjectsCondition(1, "villager", 5);

        var result = condition.Evaluate(_context);

        Assert.True(result);
    }

    [Fact]
    [Trait("Category", "Triggers")]
    public void OwnObjectsCondition_ShouldReturnFalse_WhenNotEnoughUnits()
    {
        AddUnits(3);
        var condition = new OwnObjectsCondition(1, "villager", 5);

        var result = condition.Evaluate(_context);

        Assert.False(result);
    }

    [Fact]
    [Trait("Category", "Triggers")]
    public void CreateObjectEffect_ShouldCreateUnit_WhenExecuted()
    {
        var effect = new CreateObjectEffect(1, "villager", 10, 20);

        effect.Execute(_context);

        Assert.Single(_player.UnitIds);

        var unit = _world.Entities.GetUnitById(_player.UnitIds[0]);
        Assert.NotNull(unit);
        Assert.Equal(10, unit.Position.X);
        Assert.Equal(20, unit.Position.Y);
    }

    [Fact]
    [Trait("Category", "Triggers")]
    public void Trigger_ShouldExecuteEffects_WhenConditionsMet()
    {
        AddUnits(10);
        var trigger = new Trigger
        {
            Id = "test_trigger",
            Enabled = true,
            Looping = false,
            Conditions = new List<ITriggerCondition>
            {
                new OwnObjectsCondition(1, "villager", 5)
            },
            Effects = new List<ITriggerEffect>
            {
                new CreateObjectEffect(1, "villager", 50, 50)
            }
        };

        var handler = new TriggerHandler();
        handler.RegisterTrigger(trigger);
        handler.Update(_context);

        Assert.Equal(11, _player.UnitIds.Count);
    }

    [Fact]
    [Trait("Category", "Triggers")]
    public void Trigger_ShouldNotExecuteEffects_WhenConditionsNotMet()
    {
        AddUnits(3);
        var trigger = new Trigger
        {
            Id = "test_trigger",
            Enabled = true,
            Looping = false,
            Conditions = new List<ITriggerCondition>
            {
                new OwnObjectsCondition(1, "villager", 5)
            },
            Effects = new List<ITriggerEffect>
            {
                new CreateObjectEffect(1, "villager", 50, 50)
            }
        };

        var handler = new TriggerHandler();
        handler.RegisterTrigger(trigger);
        handler.Update(_context);

        Assert.Equal(3, _player.UnitIds.Count);
    }

    [Fact]
    [Trait("Category", "Triggers")]
    public void Trigger_ShouldNotExecuteTwice_WhenNotLooping()
    {
        AddUnits(10);
        var trigger = new Trigger
        {
            Id = "test_trigger",
            Enabled = true,
            Looping = false,
            Conditions = new List<ITriggerCondition>
            {
                new OwnObjectsCondition(1, "villager", 5)
            },
            Effects = new List<ITriggerEffect>
            {
                new CreateObjectEffect(1, "villager", 50, 50)
            }
        };

        var handler = new TriggerHandler();
        handler.RegisterTrigger(trigger);

        handler.Update(_context);
        handler.Update(_context);

        Assert.Equal(11, _player.UnitIds.Count);
    }

    [Fact]
    [Trait("Category", "Triggers")]
    public void Trigger_ShouldExecuteTwice_WhenLooping()
    {
        AddUnits(10);
        var trigger = new Trigger
        {
            Id = "test_trigger",
            Enabled = true,
            Looping = true,
            Conditions = new List<ITriggerCondition>
            {
                new OwnObjectsCondition(1, "villager", 5)
            },
            Effects = new List<ITriggerEffect>
            {
                new CreateObjectEffect(1, "villager", 50, 50)
            }
        };

        var handler = new TriggerHandler();
        handler.RegisterTrigger(trigger);

        handler.Update(_context);
        handler.Update(_context);

        Assert.Equal(12, _player.UnitIds.Count);
    }

    [Fact]
    [Trait("Category", "Triggers")]
    public void ConditionFactory_ShouldCreateOwnObjectsCondition_WhenValidDefinition()
    {
        var definition = new ConditionDefinition
        {
            ConditionType = 3,
            PlayerId = 1,
            ObjectTypeId = "villager",
            Quantity = 10
        };

        var condition = ConditionFactory.Create(definition);

        Assert.IsType<OwnObjectsCondition>(condition);
    }

    [Fact]
    [Trait("Category", "Triggers")]
    public void EffectFactory_ShouldCreateCreateObjectEffect_WhenValidDefinition()
    {
        var definition = new EffectDefinition
        {
            EffectType = 1,
            PlayerId = 1,
            ObjectTypeId = "villager",
            LocationX = 10,
            LocationY = 20
        };

        var effect = EffectFactory.Create(definition);

        Assert.IsType<CreateObjectEffect>(effect);
    }

    [Fact]
    [Trait("Category", "Triggers")]
    public void WaveSystemJson_ShouldLoadCorrectly_WhenFileExists()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Data", "Triggers", "CoreTrigger", "wave_system.json");

        Assert.True(File.Exists(path), $"File not found: {path}");

        var mission = MissionLoader.Load(path);

        Assert.NotNull(mission);
        Assert.Equal("wave_system", mission.MissionId);
        Assert.Equal("Wave System", mission.Name);
        Assert.Equal(3, mission.Triggers.Count);
    }

    [Fact]
    [Trait("Category", "Triggers")]
    public void WaveSystemJson_ShouldCreateCorrectTriggers_WhenLoaded()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Data", "Triggers", "CoreTrigger", "wave_system.json");
        var mission = MissionLoader.Load(path);

        var handler = new TriggerHandler();
        foreach (var triggerDef in mission!.Triggers)
        {
            var trigger = TriggerFactory.Create(triggerDef);
            handler.RegisterTrigger(trigger);
        }

        Assert.Equal(3, handler.TriggerCount);
    }

    [Fact]
    [Trait("Category", "Triggers")]
    public void WaveSystemJson_ShouldDecodeCorrectly_WhenParsedDynamically()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Data", "Triggers", "CoreTrigger", "wave_system.json");
        var json = File.ReadAllText(path);

        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var missionId = root.GetProperty("MissionId").GetString();
        var name = root.GetProperty("Name").GetString();
        var triggers = root.GetProperty("Triggers");

        Assert.Equal("wave_system", missionId);
        Assert.Equal("Wave System", name);
        Assert.Equal(3, triggers.GetArrayLength());

        var firstTrigger = triggers[0];
        var triggerName = firstTrigger.GetProperty("TriggerName").GetString();
        Assert.Equal("wave_spawn_militia", triggerName);

        var conditions = firstTrigger.GetProperty("Conditions");
        Assert.Equal(1, conditions.GetArrayLength());

        var firstCondition = conditions[0];
        var conditionType = firstCondition.GetProperty("ConditionType").GetInt32();
        Assert.Equal(10, conditionType);
    }

    [Fact]
    [Trait("Category", "Triggers")]
    public void LastWithBuildingsCondition_ShouldReturnTrue_WhenOnePlayerHasBuildings()
    {
        var player1 = _world.GetPlayerById(1)!;
        var player2 = _world.GetPlayerById(2)!;

        var buildingDef = TestDefinitionFactory.CreateTownCenter();
        var building = BuildingFactory.Create(buildingDef, 1, new GridPosition(5, 5));
        building.IsCompleted = true;
        _world.Entities.Add(building, player1);

        var condition = new LastWithBuildingsCondition();
        var result = condition.Evaluate(_context);

        Assert.True(result);
    }

    [Fact]
    [Trait("Category", "Triggers")]
    public void LastWithBuildingsCondition_ShouldReturnFalse_WhenMultiplePlayersHaveBuildings()
    {
        var player1 = _world.GetPlayerById(1)!;
        var player2 = _world.GetPlayerById(2)!;

        var buildingDef = TestDefinitionFactory.CreateTownCenter();
        var building1 = BuildingFactory.Create(buildingDef, 1, new GridPosition(5, 5));
        building1.IsCompleted = true;
        _world.Entities.Add(building1, player1);

        var building2 = BuildingFactory.Create(buildingDef, 2, new GridPosition(35, 35));
        building2.IsCompleted = true;
        _world.Entities.Add(building2, player2);

        var condition = new LastWithBuildingsCondition();
        var result = condition.Evaluate(_context);

        Assert.False(result);
    }

    [Fact]
    [Trait("Category", "Triggers")]
    public void ScoreReachedCondition_ShouldReturnTrue_WhenScoreReached()
    {
        _player.Score = 1000;

        var condition = new ScoreReachedCondition(1, 1000);
        var result = condition.Evaluate(_context);

        Assert.True(result);
    }

    [Fact]
    [Trait("Category", "Triggers")]
    public void ScoreReachedCondition_ShouldReturnFalse_WhenScoreNotReached()
    {
        _player.Score = 500;

        var condition = new ScoreReachedCondition(1, 1000);
        var result = condition.Evaluate(_context);

        Assert.False(result);
    }

    [Fact]
    [Trait("Category", "Triggers")]
    public void EndGameEffect_ShouldFinishWorld()
    {
        var effect = new EndGameEffect();

        effect.Execute(_context);

        Assert.Equal(WorldState.Finished, _world.State);
    }

    [Fact]
    [Trait("Category", "Triggers")]
    public void ConditionFactory_ShouldCreateLastWithBuildingsCondition_WhenValidDefinition()
    {
        var definition = new ConditionDefinition
        {
            ConditionType = 20
        };

        var condition = ConditionFactory.Create(definition);

        Assert.IsType<LastWithBuildingsCondition>(condition);
    }

    [Fact]
    [Trait("Category", "Triggers")]
    public void ConditionFactory_ShouldCreateScoreReachedCondition_WhenValidDefinition()
    {
        var definition = new ConditionDefinition
        {
            ConditionType = 21,
            PlayerId = 1,
            ScoreTarget = 1000
        };

        var condition = ConditionFactory.Create(definition);

        Assert.IsType<ScoreReachedCondition>(condition);
    }

    [Fact]
    [Trait("Category", "Triggers")]
    public void EffectFactory_ShouldCreateEndGameEffect_WhenValidDefinition()
    {
        var definition = new EffectDefinition
        {
            EffectType = 99
        };

        var effect = EffectFactory.Create(definition);

        Assert.IsType<EndGameEffect>(effect);
    }
}
