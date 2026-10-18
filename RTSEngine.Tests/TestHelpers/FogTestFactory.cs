using RTSEngine.Core.Entities.Buildings;
using RTSEngine.Core.Entities.Definitions;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Settings;
using RTSEngine.Core.State;
using RTSEngine.Core.Systems;

namespace RTSEngine.Tests.TestHelpers;

/// <summary>
/// The three worlds and the two spawn helpers the visibility suite needs. They
/// were copied verbatim into four test classes; keeping one copy means a
/// change to the default definition cannot drift between them.
/// </summary>
public static class FogTestFactory
{
    public static GameWorld CreateFogWorld(int width = 40, int height = 40)
        => TestWorldFactory.CreateWorldWithTwoPlayers(
            width: width, height: height, visibility: MapVisibility.Normal);

    public static GameWorld CreateAllVisibleWorld(int width = 40, int height = 40)
        => TestWorldFactory.CreateWorldWithTwoPlayers(
            width: width, height: height, visibility: MapVisibility.AllVisible);

    public static GameWorld CreateExploredWorld(int width = 40, int height = 40)
        => TestWorldFactory.CreateWorldWithTwoPlayers(
            width: width, height: height, visibility: MapVisibility.Explored);

    public static void Update(GameWorld world)
        => VisibilitySystem.Update(SimulationTestHelper.CreateContext(world));

    public static Unit SpawnUnit(
        GameWorld world,
        int playerId,
        GridPosition position,
        int sightRange = 4)
    {
        var definition = new UnitDefinition
        {
            Id = "militia",
            Name = "Militia",
            MaxHealth = 60,
            SightRange = sightRange
        };

        var unit = new Unit(playerId, position, definition);
        world.Entities.Add(unit, world.GetPlayerById(playerId)!);
        return unit;
    }

    public static Building SpawnBuilding(
        GameWorld world,
        int playerId,
        GridPosition position,
        int sightRange = 5)
    {
        var definition = new BuildingDefinition
        {
            Id = "watch_tower",
            Name = "Watch Tower",
            MaxHealth = 500,
            Width = 1,
            Height = 1,
            SightRange = sightRange
        };

        var building = BuildingFactory.Create(definition, playerId, position);
        building.IsCompleted = true;
        building.Health.CurrentHealth = definition.MaxHealth;
        world.Entities.Add(building, world.GetPlayerById(playerId)!);
        return building;
    }
}
