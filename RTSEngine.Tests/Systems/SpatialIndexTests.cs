using RTSEngine.Core.Entities.Buildings;
using RTSEngine.Core.Entities.Definitions;
using RTSEngine.Core.Entities.Resources;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Helpers;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.State;
using RTSEngine.Core.Systems;
using RTSEngine.Tests.TestHelpers;

namespace RTSEngine.Tests.Systems;

public class SpatialIndexTests
{
    private readonly SpatialIndex _spatial = new();

    [Fact]
    public void Rebuild_ShouldIndexUnitAtPosition()
    {
        var unit = CreateUnit(1, new GridPosition(3, 5));

        _spatial.Rebuild([unit], [], []);

        Assert.Same(unit, _spatial.GetUnitAt(3, 5));
    }

    [Fact]
    public void Rebuild_ShouldReturnNull_ForEmptyPosition()
    {
        _spatial.Rebuild([], [], []);

        Assert.Null(_spatial.GetUnitAt(0, 0));
    }

    [Fact]
    public void Rebuild_ShouldIndexBuildingAtPosition()
    {
        var building = CreateBuilding(1, new GridPosition(2, 2), 1, 1);

        _spatial.Rebuild([], [building], []);

        Assert.Same(building, _spatial.GetBuildingAt(2, 2));
    }

    [Fact]
    public void Rebuild_ShouldIndexResourceAtPosition()
    {
        var tree = new Tree(new GridPosition(4, 4));

        _spatial.Rebuild([], [], [tree]);

        Assert.Same(tree, _spatial.GetResourceAt(4, 4));
    }

    [Fact]
    public void HasEntityAt_ShouldReturnTrue_ForUnit()
    {
        var unit = CreateUnit(1, new GridPosition(1, 1));

        _spatial.Rebuild([unit], [], []);

        Assert.True(_spatial.HasEntityAt(1, 1));
    }

    [Fact]
    public void HasEntityAt_ShouldReturnTrue_ForBuilding()
    {
        var building = CreateBuilding(1, new GridPosition(1, 1), 1, 1);

        _spatial.Rebuild([], [building], []);

        Assert.True(_spatial.HasEntityAt(1, 1));
    }

    [Fact]
    public void HasEntityAt_ShouldReturnTrue_ForResource()
    {
        var tree = new Tree(new GridPosition(1, 1));

        _spatial.Rebuild([], [], [tree]);

        Assert.True(_spatial.HasEntityAt(1, 1));
    }

    [Fact]
    public void HasEntityAt_ShouldReturnFalse_ForEmptyTile()
    {
        _spatial.Rebuild([], [], []);

        Assert.False(_spatial.HasEntityAt(1, 1));
    }

    [Fact]
    public void IsBuildingAt_ShouldBlockAllOccupiedTiles_ForMultiTileBuilding()
    {
        var building = CreateBuilding(1, new GridPosition(2, 2), width: 3, height: 2);

        _spatial.Rebuild([], [building], []);

        Assert.True(_spatial.IsBuildingAt(2, 2));
        Assert.True(_spatial.IsBuildingAt(3, 2));
        Assert.True(_spatial.IsBuildingAt(4, 2));
        Assert.True(_spatial.IsBuildingAt(2, 3));
        Assert.True(_spatial.IsBuildingAt(3, 3));
        Assert.True(_spatial.IsBuildingAt(4, 3));
    }

    [Fact]
    public void IsBuildingAt_ShouldNotBlock_OutsideFootprint()
    {
        var building = CreateBuilding(1, new GridPosition(2, 2), width: 3, height: 2);

        _spatial.Rebuild([], [building], []);

        Assert.False(_spatial.IsBuildingAt(1, 2));
        Assert.False(_spatial.IsBuildingAt(2, 1));
        Assert.False(_spatial.IsBuildingAt(5, 2));
        Assert.False(_spatial.IsBuildingAt(2, 4));
    }

    [Fact]
    public void IsBuildingAt_ShouldReturnFalse_ForSingleTileBuilding()
    {
        var building = CreateBuilding(1, new GridPosition(5, 5), width: 1, height: 1);

        _spatial.Rebuild([], [building], []);

        Assert.True(_spatial.IsBuildingAt(5, 5));
        Assert.False(_spatial.IsBuildingAt(6, 5));
        Assert.False(_spatial.IsBuildingAt(5, 6));
    }

    [Fact]
    public void Rebuild_ShouldNotBlockTiles_ForDeadUnit()
    {
        var unit = CreateUnit(1, new GridPosition(3, 3));
        unit.Health.TakeDamage(unit.Health.CurrentHealth);

        _spatial.Rebuild([unit], [], []);

        Assert.False(unit.IsBlocking);
    }

    [Fact]
    public void Rebuild_ShouldNotBlockTiles_ForDeadBuilding()
    {
        var building = CreateBuilding(1, new GridPosition(5, 5), width: 2, height: 2);
        building.Health.CurrentHealth = building.Health.MaxHealth;
        building.Health.TakeDamage(building.Health.MaxHealth);

        _spatial.Rebuild([], [building], []);

        Assert.False(_spatial.IsBuildingAt(5, 5));
        Assert.False(_spatial.IsBuildingAt(6, 5));
        Assert.False(_spatial.IsBuildingAt(5, 6));
        Assert.False(_spatial.IsBuildingAt(6, 6));
    }

    [Fact]
    public void Rebuild_ShouldNotIndex_DepletedResource()
    {
        var tree = new Tree(new GridPosition(4, 4));
        tree.Amount = 0;

        _spatial.Rebuild([], [], [tree]);

        Assert.Null(_spatial.GetResourceAt(4, 4));
        Assert.False(_spatial.HasEntityAt(4, 4));
    }

    [Fact]
    public void Rebuild_ShouldClearStaleData()
    {
        var unit = CreateUnit(1, new GridPosition(1, 1));
        _spatial.Rebuild([unit], [], []);
        Assert.True(_spatial.HasEntityAt(1, 1));

        _spatial.Rebuild([], [], []);
        Assert.False(_spatial.HasEntityAt(1, 1));
    }

    [Fact]
    public void Rebuild_ShouldOverwritePreviousIndex()
    {
        var unitA = CreateUnit(1, new GridPosition(1, 1));
        var unitB = CreateUnit(1, new GridPosition(5, 5));

        _spatial.Rebuild([unitA], [], []);
        Assert.Same(unitA, _spatial.GetUnitAt(1, 1));

        _spatial.Rebuild([unitB], [], []);
        Assert.Null(_spatial.GetUnitAt(1, 1));
        Assert.Same(unitB, _spatial.GetUnitAt(5, 5));
    }

    [Fact]
    public void GetResourceAt_ShouldReturnCorrectType()
    {
        var gold = new GoldMine(new GridPosition(2, 2));
        var tree = new Tree(new GridPosition(3, 3));

        _spatial.Rebuild([], [], [gold, tree]);

        Assert.Same(gold, _spatial.GetResourceAt(2, 2));
        Assert.Same(tree, _spatial.GetResourceAt(3, 3));
    }

    [Fact]
    public void MultipleUnits_SamePosition_BothIndexed()
    {
        var unitA = CreateUnit(1, new GridPosition(1, 1));
        var unitB = CreateUnit(1, new GridPosition(1, 1));

        _spatial.Rebuild([unitA, unitB], [], []);

        Assert.NotNull(_spatial.GetUnitAt(1, 1));
    }

    [Fact]
    public void Entities_DirtyFlag_ShouldAutoRebuild()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var player = world.GetPlayerById(1)!;

        var unit = TestDefinitionFactory.CreateVillager();
        var u = UnitFactory.Create(unit, 1, new GridPosition(3, 3));
        world.Entities.Add(u, player);

        Assert.True(world.Entities.Spatial.HasEntityAt(3, 3));
    }

    [Fact]
    public void Entities_DirtyFlag_ShouldAutoRebuildAfterRemove()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var player = world.GetPlayerById(1)!;

        var unit = TestDefinitionFactory.CreateVillager();
        var u = UnitFactory.Create(unit, 1, new GridPosition(3, 3));
        world.Entities.Add(u, player);
        Assert.True(world.Entities.Spatial.HasEntityAt(3, 3));

        world.Entities.Remove(u, player);
        Assert.False(world.Entities.Spatial.HasEntityAt(3, 3));
    }

    [Fact]
    public void Entities_DirtyFlag_ShouldAutoRebuildAfterResourceAdd()
    {
        var world = TestWorldFactory.CreateWorld();

        world.Entities.Add(new Tree(new GridPosition(2, 2)));

        Assert.True(world.Entities.Spatial.HasEntityAt(2, 2));
        Assert.NotNull(world.Entities.Spatial.GetResourceAt(2, 2));
    }

    [Fact]
    public void Entities_DirtyFlag_ShouldAutoRebuildAfterBuildingAdd()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var player = world.GetPlayerById(1)!;

        var definition = TestDefinitionFactory.CreateHouse();
        var building = BuildingFactory.Create(definition, 1, new GridPosition(5, 5));
        building.Health.CurrentHealth = definition.MaxHealth;
        world.Entities.Add(building, player);

        Assert.True(world.Entities.Spatial.IsBuildingAt(5, 5));
        Assert.True(world.Entities.Spatial.IsBuildingAt(6, 5));
    }

    [Fact]
    public void Entities_DirtyFlag_ShouldRebuildAfterBuildingDestruction()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var player = world.GetPlayerById(1)!;

        var definition = TestDefinitionFactory.CreateHouse();
        var building = BuildingFactory.Create(definition, 1, new GridPosition(5, 5));
        building.Health.CurrentHealth = definition.MaxHealth;
        world.Entities.Add(building, player);

        Assert.True(WorldQueries.IsTileBlocked(world, 5, 5));

        building.Health.TakeDamage(building.Health.CurrentHealth);
        world.Entities.RebuildSpatialIndex();

        Assert.False(WorldQueries.IsTileBlocked(world, 5, 5));
    }

    [Fact]
    public void Entities_DirtyFlag_ShouldRebuildAfterUnitDestruction()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var player = world.GetPlayerById(1)!;

        var def = TestDefinitionFactory.CreateVillager();
        var unit = UnitFactory.Create(def, 1, new GridPosition(3, 3));
        unit.Health.CurrentHealth = unit.Health.MaxHealth;
        world.Entities.Add(unit, player);

        Assert.True(WorldQueries.IsTileBlocked(world, 3, 3));

        unit.Health.TakeDamage(unit.Health.MaxHealth);
        world.Entities.RebuildSpatialIndex();

        Assert.False(WorldQueries.IsTileBlocked(world, 3, 3));
    }

    private static Unit CreateUnit(int ownerId, GridPosition position)
    {
        var def = new UnitDefinition
        {
            Id = "villager",
            Name = "Villager",
            MaxHealth = 50,
            MovementSpeed = 1f
        };
        return UnitFactory.Create(def, ownerId, position);
    }

    private static Building CreateBuilding(
        int ownerId, GridPosition position, int width, int height)
    {
        var definition = new BuildingDefinition
        {
            Id = "house",
            Name = "House",
            Width = width,
            Height = height,
            MaxHealth = 100
        };
        var building = BuildingFactory.Create(definition, ownerId, position);
        building.Health.CurrentHealth = definition.MaxHealth;
        return building;
    }
}
