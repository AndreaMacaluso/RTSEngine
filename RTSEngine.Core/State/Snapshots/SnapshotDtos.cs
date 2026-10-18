namespace RTSEngine.Core.State.Snapshots;

using RTSEngine.Core.Settings;

/// <summary>
    /// The game settings a save has to carry. Without them a load falls
    /// back to defaults - Visibility would turn a fog game into a map-wide one.
/// </summary>
public sealed class SettingsSnapshot
{
    public string MapPath { get; init; } = "";
    public int Width { get; init; } = 40;
    public int Height { get; init; } = 40;
    public int NumPlayers { get; init; } = 2;
    public int PopulationCap { get; init; } = 200;
    public string Speed { get; init; } = "Normal";
    public string Visibility { get; init; } = nameof(MapVisibility.Normal);
    public string Mode { get; init; } = "RandomMap";
    public string Victory { get; init; } = "Conquest";

    /// <summary>
    /// Reads Visibility back into a MapVisibility.
    ///
    /// Falls back to MapVisibility.Normal on a value that does not parse:
    /// a bad string must never turn a fog game into a map-wide one.
    /// </summary>
    public MapVisibility ToVisibility()
        => Enum.TryParse<MapVisibility>(Visibility, out var value)
            ? value
            : MapVisibility.Normal;

    public static SettingsSnapshot From(GameSettings settings)
        => new()
        {
            MapPath = settings.MapPath,
            Width = settings.Width,
            Height = settings.Height,
            NumPlayers = settings.NumPlayers,
            PopulationCap = settings.PopulationCap,
            Speed = settings.Speed.ToString(),
            Visibility = settings.Visibility.ToString(),
            Mode = settings.Mode.ToString(),
            Victory = settings.Victory.ToString()
        };

    public GameSettings ToGameSettings()
    {
        var settings = new GameSettings
        {
            MapPath = MapPath,
            Width = Width,
            Height = Height,
            NumPlayers = NumPlayers,
            PopulationCap = PopulationCap
        };

        if (Enum.TryParse<GameSpeed>(Speed, out var speed))
            settings.Speed = speed;
        settings.Visibility = ToVisibility();
        if (Enum.TryParse<GameMode>(Mode, out var mode))
            settings.Mode = mode;
        if (Enum.TryParse<VictoryType>(Victory, out var victory))
            settings.Victory = victory;

        return settings;
    }
}

public sealed class PositionDto
{
    public int X { get; init; }
    public int Y { get; init; }
}

public sealed class MapSnapshot
{
    public int Width { get; init; }
    public int Height { get; init; }
    public int[] Tiles { get; init; } = [];
    public SpawnSnapshot[] Spawns { get; init; } = [];
}

public sealed class SpawnSnapshot
{
    public int PlayerId { get; init; }
    public int X { get; init; }
    public int Y { get; init; }
}

public sealed class MovementSnapshot
{
    public int ProgressRaw { get; init; }
    public PositionDto? Destination { get; init; }
    public PositionDto? CurrentStep { get; init; }
    public PositionDto[] Path { get; init; } = [];
    public int BlockedTicks { get; init; }
    public bool NeedsRepath { get; init; }
}

public sealed class CombatSnapshot
{
    public string Phase { get; init; } = "";
    public int? TargetEntityId { get; init; }
    public int CooldownTicks { get; init; }
    public PositionDto? TargetGroundPosition { get; init; }
    public string? ResumePhase { get; init; }
    public PositionDto? ResumeDestination { get; init; }
}

public sealed class GatherSnapshot
{
    public int CurrentLoad { get; init; }
    public int? TargetResourceId { get; init; }
    public string? CarriedResource { get; init; }
    public string Phase { get; init; } = "";
    public PositionDto? DepositPosition { get; init; }
    public int WaitingForDepositTicks { get; init; }
}

public sealed class BuildSnapshot
{
    public int? BuildingId { get; init; }
    public string Phase { get; init; } = "";
}

public sealed class ProductionTaskSnapshot
{
    public string ProductId { get; init; } = "";
    public int RemainingTicks { get; init; }
}

public sealed class ProductionSnapshot
{
    public PositionDto? SpawnPoint { get; init; }
    public PositionDto? RallyPoint { get; init; }
    public ProductionTaskSnapshot[] Queue { get; init; } = [];
}

public sealed class UnitSnapshot
{
    public int Id { get; init; }
    public int OwnerId { get; init; }
    public string DefinitionId { get; init; } = "";
    public PositionDto Position { get; init; } = new();
    public int Health { get; init; }
    public string Task { get; init; } = "";
    public int DecayTicks { get; init; }
    public MovementSnapshot Movement { get; init; } = new();
    public CombatSnapshot Combat { get; init; } = new();
    public GatherSnapshot Gather { get; init; } = new();
    public BuildSnapshot Build { get; init; } = new();
}

public sealed class BuildingSnapshot
{
    public int Id { get; init; }
    public int OwnerId { get; init; }
    public string DefinitionId { get; init; } = "";
    public PositionDto Position { get; init; } = new();
    public int Health { get; init; }
    public string Task { get; init; } = "";
    public int DecayTicks { get; init; }
    public bool IsCompleted { get; init; }
    public int ConstructionProgress { get; init; }
    public CombatSnapshot Combat { get; init; } = new();
    public ProductionSnapshot Production { get; init; } = new();
}

public sealed class ResourceSnapshot
{
    public int Id { get; init; }
    public PositionDto Position { get; init; } = new();
    public string NodeType { get; init; } = "";
    public string Type { get; init; } = "";
    public int Amount { get; init; }
}

public sealed class PlayerSnapshot
{
    public int Id { get; init; }
    public string Name { get; init; } = "";
    public int Score { get; init; }
    public int Color { get; init; }
    public string Controller { get; init; } = "";
    public bool IsWinner { get; init; }
    public int Wood { get; init; }
    public int Food { get; init; }
    public int Gold { get; init; }
    public int Stone { get; init; }
    public int PopCurrent { get; init; }
    public int PopCap { get; init; }
    public int PopReserved { get; init; }
    public List<int> RememberedEnemyBuildingIds { get; init; } = [];
}

public sealed class ProjectileSnapshot
{
    public int Id { get; init; }
    public int OwnerId { get; init; }
    public int XRaw { get; init; }
    public int YRaw { get; init; }
    public int TargetXRaw { get; init; }
    public int TargetYRaw { get; init; }
    public int? TargetEntityId { get; init; }
    public int Damage { get; init; }
    public int SpeedRaw { get; init; }
    public bool IsSingleTarget { get; init; }
    public int SplashRadiusRaw { get; init; }
    public bool IsActive { get; init; }
}

public sealed class FogSnapshot
{
    public int PlayerId { get; init; }
    public int[] Tiles { get; init; } = [];
}
