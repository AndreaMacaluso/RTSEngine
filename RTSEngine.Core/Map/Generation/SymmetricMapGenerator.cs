using RTSEngine.Core.Map.Definitions;
using RTSEngine.Core.Map.Runtime;

namespace RTSEngine.Core.Map.Generation;

public static class SymmetricMapGenerator
{
    private const int MaxPlacementAttempts = 200;

    public static MapData Generate(
        string name,
        MapGenerationDefinition definition)
    {
        Validate(definition);

        var random = new Random(definition.Seed);
        var terrain = CreateTerrain(definition.Width, definition.Height);
        var firstSpawn = new GridPosition(definition.SpawnInset, definition.SpawnInset);
        var secondSpawn = Mirror(firstSpawn, definition);
        var firstBaseCenter = new GridPosition(firstSpawn.X + 1, firstSpawn.Y + 1);
        var secondBaseCenter = new GridPosition(secondSpawn.X + 1, secondSpawn.Y + 1);

        GenerateTerrainPatches(
            terrain, definition, firstBaseCenter, secondBaseCenter, random);

        var resources = GenerateResources(
            definition, firstBaseCenter, secondBaseCenter, terrain, random);

        return new MapData
        {
            Name = name,
            Width = definition.Width,
            Height = definition.Height,
            Rows = ToRows(terrain),
            Resources = resources,
            Spawns =
            [
                new SpawnPointDefinition { PlayerId = 1, X = firstSpawn.X, Y = firstSpawn.Y },
                new SpawnPointDefinition { PlayerId = 2, X = secondSpawn.X, Y = secondSpawn.Y }
            ]
        };
    }

    private static List<ResourceDefinition> GenerateResources(
        MapGenerationDefinition definition,
        GridPosition firstBaseCenter,
        GridPosition secondBaseCenter,
        char[,] terrain,
        Random random)
    {
        var resources = new List<ResourceDefinition>();
        var occupied = new HashSet<GridPosition>();

        foreach (var cluster in definition.ResourceClusters)
        {
            for (var index = 0; index < cluster.ClustersPerPlayer; index++)
            {
                var positions = FindClusterPositions(
                    definition, cluster, firstBaseCenter, secondBaseCenter,
                    terrain, occupied, random);

                foreach (var position in positions)
                {
                    var mirroredPosition = Mirror(position, definition);
                    occupied.Add(position);
                    occupied.Add(mirroredPosition);
                    resources.Add(CreateResource(cluster.Type, position));
                    resources.Add(CreateResource(cluster.Type, mirroredPosition));
                }
            }
        }

        return resources;
    }

    private static List<GridPosition> FindClusterPositions(
        MapGenerationDefinition definition,
        ResourceClusterDefinition cluster,
        GridPosition firstBaseCenter,
        GridPosition secondBaseCenter,
        char[,] terrain,
        HashSet<GridPosition> occupied,
        Random random)
    {
        for (var attempt = 0; attempt < MaxPlacementAttempts; attempt++)
        {
            var center = new GridPosition(
                random.Next(1, definition.Width - 1),
                random.Next(1, definition.Height - 1));

            if (!IsInFirstPlayerRegion(center, firstBaseCenter, secondBaseCenter) ||
                !IsAtRequestedDistance(center, firstBaseCenter, cluster))
            {
                continue;
            }

            var positions = TryCreateCluster(
                definition, cluster, center, firstBaseCenter, secondBaseCenter,
                terrain, occupied, random);

            if (positions.Count == cluster.NodesPerCluster)
            {
                return positions;
            }
        }

        throw new InvalidOperationException(
            $"Unable to place {cluster.Type} cluster after {MaxPlacementAttempts} attempts.");
    }

    private static List<GridPosition> TryCreateCluster(
        MapGenerationDefinition definition,
        ResourceClusterDefinition cluster,
        GridPosition center,
        GridPosition firstBaseCenter,
        GridPosition secondBaseCenter,
        char[,] terrain,
        HashSet<GridPosition> occupied,
        Random random)
    {
        var positions = new List<GridPosition>();

        for (var attempt = 0;
             attempt < MaxPlacementAttempts && positions.Count < cluster.NodesPerCluster;
             attempt++)
        {
            var position = new GridPosition(
                center.X + random.Next(-cluster.ClusterRadius, cluster.ClusterRadius + 1),
                center.Y + random.Next(-cluster.ClusterRadius, cluster.ClusterRadius + 1));

            if (IsValidResourcePosition(
                    position, definition, cluster, firstBaseCenter, secondBaseCenter,
                    terrain, occupied, positions))
            {
                positions.Add(position);
            }
        }

        return positions;
    }

    private static bool IsValidResourcePosition(
        GridPosition position,
        MapGenerationDefinition definition,
        ResourceClusterDefinition cluster,
        GridPosition firstBaseCenter,
        GridPosition secondBaseCenter,
        char[,] terrain,
        HashSet<GridPosition> occupied,
        List<GridPosition> pendingPositions)
    {
        if (!IsInsideBounds(position, definition) ||
            !IsInFirstPlayerRegion(position, firstBaseCenter, secondBaseCenter) ||
            !IsAtRequestedDistance(position, firstBaseCenter, cluster))
        {
            return false;
        }

        var mirroredPosition = Mirror(position, definition);
        if (!IsWalkableTerrain(terrain[position.Y, position.X]) ||
            !IsWalkableTerrain(terrain[mirroredPosition.Y, mirroredPosition.X]) ||
            Distance(position, firstBaseCenter) <= definition.BaseClearRadius ||
            Distance(mirroredPosition, secondBaseCenter) <= definition.BaseClearRadius)
        {
            return false;
        }

        return IsSeparated(position, definition, occupied) &&
               IsSeparated(mirroredPosition, definition, occupied) &&
               IsSeparated(position, definition, pendingPositions) &&
               IsSeparated(mirroredPosition, definition, pendingPositions);
    }

    private static bool IsSeparated(
        GridPosition position,
        MapGenerationDefinition definition,
        IEnumerable<GridPosition> otherPositions)
    {
        return otherPositions.All(other =>
            Distance(position, other) > definition.ResourceSeparation);
    }

    private static void GenerateTerrainPatches(
        char[,] terrain,
        MapGenerationDefinition definition,
        GridPosition firstBaseCenter,
        GridPosition secondBaseCenter,
        Random random)
    {
        foreach (var patch in definition.TerrainPatches)
        {
            for (var index = 0; index < patch.CountPerSide; index++)
            {
                var center = FindTerrainPatchCenter(
                    definition, firstBaseCenter, secondBaseCenter, random);
                var radius = random.Next(patch.MinRadius, patch.MaxRadius + 1);
                var symbol = ToTerrainSymbol(patch.Type);

                ApplyPatch(terrain, center, radius, symbol, definition);
                ApplyPatch(terrain, Mirror(center, definition), radius, symbol, definition);
            }
        }
    }

    private static GridPosition FindTerrainPatchCenter(
        MapGenerationDefinition definition,
        GridPosition firstBaseCenter,
        GridPosition secondBaseCenter,
        Random random)
    {
        for (var attempt = 0; attempt < MaxPlacementAttempts; attempt++)
        {
            var center = new GridPosition(
                random.Next(1, definition.Width - 1),
                random.Next(1, definition.Height - 1));
            var mirroredCenter = Mirror(center, definition);

            if (Distance(center, firstBaseCenter) > definition.BaseClearRadius &&
                Distance(mirroredCenter, secondBaseCenter) > definition.BaseClearRadius)
            {
                return center;
            }
        }

        throw new InvalidOperationException("Unable to place terrain patch.");
    }

    private static void ApplyPatch(
        char[,] terrain,
        GridPosition center,
        int radius,
        char symbol,
        MapGenerationDefinition definition)
    {
        for (var y = center.Y - radius; y <= center.Y + radius; y++)
        {
            for (var x = center.X - radius; x <= center.X + radius; x++)
            {
                if (x < 0 || y < 0 || x >= definition.Width || y >= definition.Height)
                {
                    continue;
                }

                if (Distance(new GridPosition(x, y), center) <= radius)
                {
                    terrain[y, x] = symbol;
                }
            }
        }
    }

    private static bool IsInFirstPlayerRegion(
        GridPosition position,
        GridPosition firstBaseCenter,
        GridPosition secondBaseCenter)
    {
        return Distance(position, firstBaseCenter) <
               Distance(position, secondBaseCenter);
    }

    private static bool IsInsideBounds(
        GridPosition position,
        MapGenerationDefinition definition)
    {
        return position.X >= 0 && position.Y >= 0 &&
               position.X < definition.Width && position.Y < definition.Height;
    }

    private static bool IsAtRequestedDistance(
        GridPosition position,
        GridPosition baseCenter,
        ResourceClusterDefinition cluster)
    {
        var distance = Distance(position, baseCenter);
        return distance >= cluster.MinDistanceFromSpawn &&
               distance <= cluster.MaxDistanceFromSpawn;
    }

    private static int Distance(GridPosition first, GridPosition second)
    {
        return Math.Max(Math.Abs(first.X - second.X), Math.Abs(first.Y - second.Y));
    }

    private static GridPosition Mirror(
        GridPosition position,
        MapGenerationDefinition definition)
    {
        return new GridPosition(
            definition.Width - 1 - position.X,
            definition.Height - 1 - position.Y);
    }

    private static ResourceDefinition CreateResource(string type, GridPosition position)
    {
        return new ResourceDefinition { Type = type, X = position.X, Y = position.Y };
    }

    private static char[,] CreateTerrain(int width, int height)
    {
        var terrain = new char[height, width];
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                terrain[y, x] = 'G';
            }
        }

        return terrain;
    }

    private static List<string> ToRows(char[,] terrain)
    {
        var rows = new List<string>();
        for (var y = 0; y < terrain.GetLength(0); y++)
        {
            var row = new char[terrain.GetLength(1)];
            for (var x = 0; x < terrain.GetLength(1); x++)
            {
                row[x] = terrain[y, x];
            }

            rows.Add(new string(row));
        }

        return rows;
    }

    private static char ToTerrainSymbol(string type)
    {
        return type.ToLowerInvariant() switch
        {
            "sand" => 'S',
            _ => throw new InvalidOperationException(
                $"Unsupported generated terrain type '{type}'.")
        };
    }

    private static bool IsWalkableTerrain(char symbol)
    {
        return symbol is 'G' or 'S';
    }

    private static void Validate(MapGenerationDefinition definition)
    {
        if (definition.Width < 20 || definition.Height < 20)
        {
            throw new InvalidOperationException("Generated maps must be at least 20 by 20 tiles.");
        }

        if (definition.SpawnInset < 1 ||
            definition.SpawnInset >= definition.Width / 2 ||
            definition.SpawnInset >= definition.Height / 2)
        {
            throw new InvalidOperationException("Spawn inset leaves no room for both bases.");
        }

        foreach (var cluster in definition.ResourceClusters)
        {
            if (cluster.NodesPerCluster < 1 ||
                cluster.ClustersPerPlayer < 1 ||
                cluster.MinDistanceFromSpawn <= definition.BaseClearRadius ||
                cluster.MaxDistanceFromSpawn < cluster.MinDistanceFromSpawn)
            {
                throw new InvalidOperationException(
                    $"Invalid cluster definition for '{cluster.Type}'.");
            }
        }
    }
}
