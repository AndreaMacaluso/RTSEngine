using RTSEngine.Core.Map.Definitions;
using RTSEngine.Core.Map.Generation;
using RTSEngine.Core.Map.Loading;
using RTSEngine.Core.Map.Runtime;

namespace RTSEngine.Tests.Map;

public class SymmetricMapGeneratorTests
{
    [Fact]
    public void Generate_WithSameSeed_ProducesIdenticalMap()
    {
        var definition = CreateDefinition();

        var first = SymmetricMapGenerator.Generate("generated", definition);
        var second = SymmetricMapGenerator.Generate("generated", definition);

        Assert.Equal(first.Rows, second.Rows);
        Assert.Equal(
            first.Resources.Select(resource => (resource.Type, resource.X, resource.Y)),
            second.Resources.Select(resource => (resource.Type, resource.X, resource.Y)));
        Assert.Equal(
            first.Spawns.Select(spawn => (spawn.PlayerId, spawn.X, spawn.Y)),
            second.Spawns.Select(spawn => (spawn.PlayerId, spawn.X, spawn.Y)));
    }

    [Fact]
    public void Generate_CreatesMirroredBalancedResourceClusters()
    {
        var definition = CreateDefinition();

        var map = SymmetricMapGenerator.Generate("generated", definition);

        Assert.Equal(20, map.Resources.Count(resource => resource.Type == "tree"));
        Assert.Equal(20, map.Resources.Count(resource => resource.Type == "berry_bush"));
        Assert.Equal(6, map.Resources.Count(resource => resource.Type == "gold_mine"));
        Assert.Equal(8, map.Resources.Count(resource => resource.Type == "stone_mine"));

        foreach (var resource in map.Resources)
        {
            Assert.Contains(map.Resources, candidate =>
                candidate.Type == resource.Type &&
                candidate.X == definition.Width - 1 - resource.X &&
                candidate.Y == definition.Height - 1 - resource.Y);
        }
    }

    [Fact]
    public void Generate_LeavesTheStartingBaseAreaFreeOfResources()
    {
        var definition = CreateDefinition();
        var map = SymmetricMapGenerator.Generate("generated", definition);
        var baseCenters = map.Spawns.Select(spawn =>
            new GridPosition(spawn.X + 1, spawn.Y + 1));

        foreach (var resource in map.Resources)
        {
            var position = new GridPosition(resource.X, resource.Y);

            Assert.All(baseCenters, center =>
                Assert.True(Distance(position, center) > definition.BaseClearRadius));
        }
    }

    [Fact]
    public void Generate_CreatesMirroredSpawns()
    {
        var definition = CreateDefinition();
        var map = SymmetricMapGenerator.Generate("generated", definition);

        var first = map.Spawns[0];
        var second = map.Spawns[1];

        Assert.Equal(definition.SpawnInset, first.X);
        Assert.Equal(definition.SpawnInset, first.Y);
        Assert.Equal(definition.Width - 1 - first.X, second.X);
        Assert.Equal(definition.Height - 1 - first.Y, second.Y);
    }

    [Fact]
    public void WorldBuilder_UsesGenerationDefinitionWhenMapHasNoStaticTiles()
    {
        var world = WorldBuilder.Build(new MapData
        {
            Name = "generated",
            Generation = CreateDefinition()
        });

        Assert.Equal(40, world.Map.Width);
        Assert.Equal(40, world.Map.Height);
        Assert.Equal(2, world.Spawns.Count);
        Assert.Equal(54, world.Resources.Count);
    }

    private static MapGenerationDefinition CreateDefinition()
    {
        return new MapGenerationDefinition
        {
            Seed = 48291,
            Width = 40,
            Height = 40,
            SpawnInset = 5,
            BaseClearRadius = 3,
            ResourceSeparation = 1,
            ResourceClusters =
            [
                new ResourceClusterDefinition
                {
                    Type = "tree",
                    ClustersPerPlayer = 2,
                    NodesPerCluster = 5,
                    MinDistanceFromSpawn = 5,
                    MaxDistanceFromSpawn = 12,
                    ClusterRadius = 2
                },
                new ResourceClusterDefinition
                {
                    Type = "berry_bush",
                    ClustersPerPlayer = 2,
                    NodesPerCluster = 5,
                    MinDistanceFromSpawn = 4,
                    MaxDistanceFromSpawn = 12,
                    ClusterRadius = 2
                },
                new ResourceClusterDefinition
                {
                    Type = "gold_mine",
                    ClustersPerPlayer = 1,
                    NodesPerCluster = 3,
                    MinDistanceFromSpawn = 12,
                    MaxDistanceFromSpawn = 16
                },
                new ResourceClusterDefinition
                {
                    Type = "stone_mine",
                    ClustersPerPlayer = 1,
                    NodesPerCluster = 4,
                    MinDistanceFromSpawn = 12,
                    MaxDistanceFromSpawn = 16
                }
            ]
        };
    }

    private static int Distance(GridPosition first, GridPosition second)
    {
        return Math.Max(Math.Abs(first.X - second.X), Math.Abs(first.Y - second.Y));
    }
}
