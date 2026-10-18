using System.Text.Json;
using System.Text.Json.Serialization;
using RTSEngine.Core.Entities;
using RTSEngine.Core.Entities.Buildings;
using RTSEngine.Core.Entities.Definitions;
using RTSEngine.Core.Entities.Resources;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Entities.States;
using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Helpers;
using RTSEngine.Core.Map.Definitions;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Map.Visibility;
using RTSEngine.Core.Players;
// The member `Settings` below shadows this namespace, so inside GameSnapshot
// the settings types have to stay unqualified (MapVisibility, GameSettings, ...).
using RTSEngine.Core.Settings;
using RTSEngine.Core.State.Snapshots;

namespace RTSEngine.Core.State;

public sealed class GameSnapshot
{
    public const int CurrentFormatVersion = 1;

    public int Tick { get; init; }

    /// <summary>
    /// Version of the save format, written into every save file.
    /// Defaults to 0 so a file without the field is rejected on load
    /// instead of being silently read as current.
    /// </summary>
    public int FormatVersion { get; init; }
    public string State { get; init; } = "";
    public MapSnapshot Map { get; init; } = new();
    public UnitSnapshot[] Units { get; init; } = [];
    public BuildingSnapshot[] Buildings { get; init; } = [];
    public ResourceSnapshot[] Resources { get; init; } = [];
    public PlayerSnapshot[] Players { get; init; } = [];
    public ProjectileSnapshot[] Projectiles { get; init; } = [];

    /// <summary>
    /// Full grid state per player, for lockstep resync. The per-player
    /// PlayerView is derived from this, not the other way round.
    /// </summary>
    public FogSnapshot[] Fog { get; init; } = [];

    /// <summary>
    /// Game settings: without them a load reverts to defaults
    /// and a fog game becomes a map-wide one.
    ///
    /// Visibility is taken from the GameWorld, not from the settings
    /// passed in — the world is where the mode was actually applied.
    /// </summary>
    public SettingsSnapshot Settings { get; init; } = new();

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        WriteIndented = false
    };

    public static GameSnapshot Capture(GameWorld world, GameSettings? settings = null)
    {
        return new GameSnapshot
        {
            FormatVersion = CurrentFormatVersion,
            Tick = world.CurrentTick,
            State = world.State.ToString(),
            Settings = CaptureSettings(world, settings),
            Map = CaptureMap(world),
            Units = world.Entities.Units
                .Select(CaptureUnit)
                .ToArray(),
            Buildings = world.Entities.Buildings
                .Select(CaptureBuilding)
                .ToArray(),
            Resources = world.Entities.Resources
                .Select(CaptureResource)
                .ToArray(),
            Players = world.Players
                .Select(CapturePlayer)
                .ToArray(),
            Projectiles = world.Projectiles.All
                .Select(CaptureProjectile)
                .ToArray(),
            Fog = world.Fog.States
                .OrderBy(kvp => kvp.Key)
                .Select(kvp => new FogSnapshot
                {
                    PlayerId = kvp.Key,
                    Tiles = kvp.Value.Flatten()
                })
                .ToArray()
        };
    }

    private static SettingsSnapshot CaptureSettings(
        GameWorld world,
        GameSettings? settings)
    {
        var snapshot = settings is null
            ? new SettingsSnapshot()
            : SettingsSnapshot.From(settings);

        // Visibility comes from the world, not from these settings: the world
        // is where the mode was applied.
        return new SettingsSnapshot
        {
            MapPath = snapshot.MapPath,
            Width = snapshot.Width,
            Height = snapshot.Height,
            NumPlayers = snapshot.NumPlayers,
            PopulationCap = snapshot.PopulationCap,
            Speed = snapshot.Speed,
            Visibility = world.Fog.Mode.ToString(),
            Mode = snapshot.Mode,
            Victory = snapshot.Victory
        };
    }

    private static MapSnapshot CaptureMap(GameWorld world)
    {
        var map = world.Map;
        return new MapSnapshot
        {
            Width = map.Width,
            Height = map.Height,
            Tiles = FlattenGrid(map.Width, map.Height, (x, y) => (int)map.GetTile(x, y).TerrainType),
            Spawns = world.Spawns
                .Select(s => new SpawnSnapshot
                {
                    PlayerId = s.PlayerId,
                    X = s.X,
                    Y = s.Y
                })
                .ToArray()
        };
    }

    private static UnitSnapshot CaptureUnit(Unit u)
    {
        return new UnitSnapshot
        {
            Id = u.Id,
            OwnerId = u.OwnerId,
            DefinitionId = u.Definition.Id,
            Position = new PositionDto { X = u.Position.X, Y = u.Position.Y },
            Health = u.Health.CurrentHealth,
            Task = u.CurrentTask.ToString(),
            DecayTicks = u.DecayTicksRemaining,
            Movement = new MovementSnapshot
            {
                ProgressRaw = u.Movement.Progress.Raw,
                Destination = ToPosition(u.Movement.Destination),
                CurrentStep = ToPosition(u.Movement.CurrentStep),
                Path = u.Movement.PathQueue.Select(p => new PositionDto { X = p.X, Y = p.Y }).ToArray(),
                BlockedTicks = u.Movement.BlockedTicks,
                NeedsRepath = u.Movement.NeedsRepath
            },
            Combat = CaptureCombat(u.Combat),
            Gather = new GatherSnapshot
            {
                CurrentLoad = u.Gather.CurrentLoad,
                TargetResourceId = u.Gather.TargetResourceId,
                CarriedResource = u.Gather.CarriedResource?.ToString(),
                Phase = u.Gather.Phase.ToString(),
                DepositPosition = ToPosition(u.Gather.DepositPosition),
                WaitingForDepositTicks = u.Gather.WaitingForDepositTicks
            },
            Build = new BuildSnapshot
            {
                BuildingId = u.Build.BuildingId,
                Phase = u.Build.Phase.ToString()
            }
        };
    }

    private static BuildingSnapshot CaptureBuilding(Building b)
    {
        return new BuildingSnapshot
        {
            Id = b.Id,
            OwnerId = b.OwnerId,
            DefinitionId = b.Definition.Id,
            Position = new PositionDto { X = b.Position.X, Y = b.Position.Y },
            Health = b.Health.CurrentHealth,
            Task = b.CurrentTask.ToString(),
            DecayTicks = b.DecayTicksRemaining,
            IsCompleted = b.IsCompleted,
            ConstructionProgress = b.ConstructionProgress,
            Combat = CaptureCombat(b.Combat),
            Production = new ProductionSnapshot
            {
                SpawnPoint = ToPosition(b.Production.SpawnPoint),
                RallyPoint = ToPosition(b.Production.RallyPoint),
                Queue = CaptureProductionQueue(b.Production)
            }
        };
    }

    private static ProductionTaskSnapshot[] CaptureProductionQueue(ProductionState production)
    {
        return production.AllTasks
            .Select(t => new ProductionTaskSnapshot
            {
                ProductId = t.ProductId,
                RemainingTicks = t.RemainingTicks
            })
            .ToArray();
    }

    private static CombatSnapshot CaptureCombat(CombatState combat)
    {
        return new CombatSnapshot
        {
            Phase = combat.Phase.ToString(),
            TargetEntityId = combat.TargetEntityId,
            CooldownTicks = combat.CooldownTicks,
            TargetGroundPosition = ToPosition(combat.TargetGroundPosition),
            ResumePhase = combat.ResumePhase?.ToString(),
            ResumeDestination = ToPosition(combat.ResumeDestination)
        };
    }

    private static ResourceSnapshot CaptureResource(ResourceNode r)
    {
        return new ResourceSnapshot
        {
            Id = r.Id,
            Position = new PositionDto { X = r.Position.X, Y = r.Position.Y },
            NodeType = r.GetType().Name,
            Type = r.ResourceType.ToString(),
            Amount = r.Amount
        };
    }

    private static PlayerSnapshot CapturePlayer(Player p)
    {
        return new PlayerSnapshot
        {
            Id = p.Id,
            Name = p.Name,
            Score = p.Score,
            Color = (int)p.Color,
            Controller = p.Controller.ToString(),
            IsWinner = p.IsWinner,
            Wood = p.Economy.Get(ResourceType.Wood),
            Food = p.Economy.Get(ResourceType.Food),
            Gold = p.Economy.Get(ResourceType.Gold),
            Stone = p.Economy.Get(ResourceType.Stone),
            PopCurrent = p.Population.Current,
            PopCap = p.Population.Capacity,
            PopReserved = p.Population.Reserved,
            // Sorted: HashSet order is not an order, and the DTO has to be
            // bit-identical on two clients.
            RememberedEnemyBuildingIds = p.RememberedEnemyBuildingIds.OrderBy(id => id).ToList()
        };
    }

    private static ProjectileSnapshot CaptureProjectile(Projectile p)
    {
        return new ProjectileSnapshot
        {
            Id = p.Id,
            OwnerId = p.OwnerId,
            XRaw = p.X.Raw,
            YRaw = p.Y.Raw,
            TargetXRaw = p.TargetX.Raw,
            TargetYRaw = p.TargetY.Raw,
            TargetEntityId = p.TargetEntityId,
            Damage = p.Damage,
            SpeedRaw = p.Speed.Raw,
            IsSingleTarget = p.IsSingleTarget,
            SplashRadiusRaw = p.SplashRadius.Raw,
            IsActive = p.IsActive
        };
    }

    private static PositionDto? ToPosition(GridPosition? position)
    {
        return position is GridPosition p
            ? new PositionDto { X = p.X, Y = p.Y }
            : null;
    }

    private static int[] FlattenGrid(int width, int height, Func<int, int, int> getter)
    {
        var result = new int[width * height];
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                result[y * width + x] = getter(x, y);
        return result;
    }

    public static GameWorld Restore(
        GameSnapshot snapshot,
        UnitDefinitionRepository unitRepo,
        BuildingDefinitionRepository buildingRepo)
    {
        var map = RestoreMap(snapshot.Map);
        var spawns = snapshot.Map.Spawns
            .Select(s => new SpawnPointDefinition
            {
                PlayerId = s.PlayerId,
                X = s.X,
                Y = s.Y
            })
            .ToList();

        var visibility = snapshot.Settings.ToVisibility();

        var world = new GameWorld(map, null, spawns, visibility: visibility);

        RestorePlayers(world, snapshot.Players);
        RestoreFog(world, snapshot.Fog);
        RestoreResources(world, snapshot.Resources);
        RestoreBuildings(world, snapshot.Buildings, buildingRepo);
        RestoreUnits(world, snapshot.Units, unitRepo);
        RestoreProjectiles(world, snapshot.Projectiles);

        int maxEntityId = snapshot.Units.Select(u => u.Id)
            .Concat(snapshot.Buildings.Select(b => b.Id))
            .Concat(snapshot.Resources.Select(r => r.Id))
            .DefaultIfEmpty(0)
            .Max();
        world.Entities.SetNextEntityId(maxEntityId + 1);

        world.RestoreTick(snapshot.Tick);
        world.RestoreState(Enum.TryParse<WorldState>(snapshot.State, out var state)
            ? state
            : WorldState.Running);

        return world;
    }

    private static TileMap RestoreMap(MapSnapshot snapshot)
    {
        var map = new TileMap(snapshot.Width, snapshot.Height);
        for (int y = 0; y < snapshot.Height; y++)
        {
            for (int x = 0; x < snapshot.Width; x++)
            {
                map.SetTile(x, y, new Tile
                {
                    TerrainType = (TileType)snapshot.Tiles[y * snapshot.Width + x]
                });
            }
        }
        return map;
    }

    private static void RestorePlayers(GameWorld world, PlayerSnapshot[] snapshots)
    {
        foreach (var ps in snapshots)
        {
            var player = new Player(
                ps.Id,
                ps.Name,
                (ConsoleColor)ps.Color,
                Enum.Parse<PlayerControllerType>(ps.Controller))
            {
                Score = ps.Score,
                IsWinner = ps.IsWinner
            };
            player.Economy.Add(ResourceType.Wood, ps.Wood);
            player.Economy.Add(ResourceType.Food, ps.Food);
            player.Economy.Add(ResourceType.Gold, ps.Gold);
            player.Economy.Add(ResourceType.Stone, ps.Stone);
            player.Population.Current = ps.PopCurrent;
            player.Population.Capacity = ps.PopCap;
            player.Population.Reserved = ps.PopReserved;
            foreach (var rememberedId in ps.RememberedEnemyBuildingIds ?? [])
                player.RememberedEnemyBuildingIds.Add(rememberedId);
            world.AddPlayer(player);
        }
    }

    private static void RestoreFog(GameWorld world, FogSnapshot[] snapshots)
    {
        if (!world.Fog.IsEnabled)
            return;

        foreach (var fs in snapshots)
        {
            var state = world.Fog.GetState(fs.PlayerId);
            if (state == null)
                continue;

            for (int y = 0; y < state.Height; y++)
            {
                for (int x = 0; x < state.Width; x++)
                {
                    state.Set(new GridPosition(x, y), (TileVisibility)fs.Tiles[y * state.Width + x]);
                }
            }

            // Derived state: rebuilt here, never in the game loop.
            state.RebuildVisibleIndex();
        }
    }

    private static void RestoreResources(GameWorld world, ResourceSnapshot[] snapshots)
    {
        foreach (var rs in snapshots)
        {
            var position = new GridPosition(rs.Position.X, rs.Position.Y);
            var resource = CreateResourceByType(rs.NodeType, position);
            resource.ResourceType = Enum.Parse<ResourceType>(rs.Type);
            resource.Amount = rs.Amount;
            world.Entities.AddRestored(resource, rs.Id);
        }
    }

    private static ResourceNode CreateResourceByType(string nodeType, GridPosition position)
    {
        return nodeType switch
        {
            nameof(Tree) => new Tree(position),
            nameof(BerryBush) => new BerryBush(position),
            nameof(GoldMine) => new GoldMine(position),
            nameof(StoneMine) => new StoneMine(position),
            _ => throw new InvalidOperationException($"Unknown resource node type: {nodeType}")
        };
    }

    private static void RestoreBuildings(
        GameWorld world,
        BuildingSnapshot[] snapshots,
        BuildingDefinitionRepository buildingRepo)
    {
        foreach (var bs in snapshots)
        {
            var definition = buildingRepo.Get(bs.DefinitionId);
            var building = BuildingFactory.Create(
                definition,
                bs.OwnerId,
                new GridPosition(bs.Position.X, bs.Position.Y));

            building.Health.CurrentHealth = bs.Health;
            building.IsCompleted = bs.IsCompleted;
            building.ConstructionProgress = bs.ConstructionProgress;
            building.CurrentTask = Enum.Parse<EntityState>(bs.Task);
            building.DecayTicksRemaining = bs.DecayTicks;

            RestoreCombat(building.Combat, bs.Combat);

            building.Production.SpawnPoint = FromPosition(bs.Production.SpawnPoint);
            building.Production.RallyPoint = FromPosition(bs.Production.RallyPoint);
            foreach (var task in bs.Production.Queue)
            {
                building.Production.Add(new ProductionTask(task.ProductId, task.RemainingTicks));
            }

            var player = world.GetPlayerById(bs.OwnerId);
            if (player == null) continue;
            world.Entities.AddRestored(building, player, bs.Id);
        }
    }

    private static void RestoreUnits(
        GameWorld world,
        UnitSnapshot[] snapshots,
        UnitDefinitionRepository unitRepo)
    {
        foreach (var us in snapshots)
        {
            var definition = unitRepo.Get(us.DefinitionId);
            var unit = UnitFactory.Create(
                definition,
                us.OwnerId,
                new GridPosition(us.Position.X, us.Position.Y));

            unit.Health.CurrentHealth = us.Health;
            unit.CurrentTask = Enum.Parse<EntityState>(us.Task);
            unit.DecayTicksRemaining = us.DecayTicks;

            unit.Movement.Progress = new FixedPoint(us.Movement.ProgressRaw);
            unit.Movement.Destination = FromPosition(us.Movement.Destination);
            unit.Movement.CurrentStep = FromPosition(us.Movement.CurrentStep);
            unit.Movement.PathQueue.Clear();
            foreach (var step in us.Movement.Path)
                unit.Movement.PathQueue.Enqueue(FromPosition(step)!.Value);
            unit.Movement.BlockedTicks = us.Movement.BlockedTicks;
            unit.Movement.NeedsRepath = us.Movement.NeedsRepath;

            RestoreCombat(unit.Combat, us.Combat);

            unit.Gather.CurrentLoad = us.Gather.CurrentLoad;
            unit.Gather.TargetResourceId = us.Gather.TargetResourceId;
            unit.Gather.CarriedResource = us.Gather.CarriedResource != null
                ? Enum.Parse<ResourceType>(us.Gather.CarriedResource)
                : null;
            unit.Gather.Phase = Enum.Parse<GatherPhase>(us.Gather.Phase);
            unit.Gather.DepositPosition = FromPosition(us.Gather.DepositPosition);
            unit.Gather.WaitingForDepositTicks = us.Gather.WaitingForDepositTicks;

            unit.Build.BuildingId = us.Build.BuildingId;
            unit.Build.Phase = Enum.Parse<BuildPhase>(us.Build.Phase);

            var player = world.GetPlayerById(us.OwnerId);
            if (player == null) continue;
            world.Entities.AddRestored(unit, player, us.Id);
        }
    }

    private static void RestoreCombat(CombatState combat, CombatSnapshot snapshot)
    {
        combat.Phase = Enum.Parse<CombatPhase>(snapshot.Phase);
        combat.TargetEntityId = snapshot.TargetEntityId;
        combat.CooldownTicks = snapshot.CooldownTicks;
        combat.TargetGroundPosition = FromPosition(snapshot.TargetGroundPosition);
        combat.ResumePhase = string.IsNullOrEmpty(snapshot.ResumePhase)
            || !Enum.TryParse<CombatPhase>(snapshot.ResumePhase, out var resumePhase)
                ? null
                : resumePhase;
        combat.ResumeDestination = FromPosition(snapshot.ResumeDestination);
    }

    private static void RestoreProjectiles(GameWorld world, ProjectileSnapshot[] snapshots)
    {
        var projectiles = snapshots
            .Select(p => new Projectile
            {
                Id = p.Id,
                OwnerId = p.OwnerId,
                X = new FixedPoint(p.XRaw),
                Y = new FixedPoint(p.YRaw),
                TargetX = new FixedPoint(p.TargetXRaw),
                TargetY = new FixedPoint(p.TargetYRaw),
                TargetEntityId = p.TargetEntityId,
                Damage = p.Damage,
                Speed = new FixedPoint(p.SpeedRaw),
                IsSingleTarget = p.IsSingleTarget,
                SplashRadius = new FixedPoint(p.SplashRadiusRaw),
                IsActive = p.IsActive
            })
            .ToList();

        int nextId = projectiles.Count > 0 ? projectiles.Max(p => p.Id) + 1 : 1;
        world.Projectiles.RestoreProjectiles(projectiles, nextId);
    }

    private static GridPosition? FromPosition(PositionDto? position)
    {
        return position is PositionDto p
            ? new GridPosition(p.X, p.Y)
            : null;
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(this, SerializerOptions);
    }

    public static GameSnapshot? Deserialize(string json)
    {
        return JsonSerializer.Deserialize<GameSnapshot>(json, SerializerOptions);
    }
}