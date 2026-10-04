using RTSEngine.Core.Players.States;
namespace RTSEngine.Core.Players;

public sealed class Player
{
    public int Id { get; }
    public string Name { get; set; } = "";
    public ConsoleColor Color { get; set; }
    public PlayerControllerType Controller { get; set; }
    public int Score { get; set; } = 0;
    public bool IsWinner { get; internal set; }
    public EconomyState Economy { get; }
    public PopulationState Population { get; }
    public PlayerAIState AI { get; }

    private readonly List<int> _unitIds = [];
    private readonly List<int> _buildingIds = [];

    public IReadOnlyList<int> UnitIds => _unitIds;
    public IReadOnlyList<int> BuildingIds => _buildingIds;

    /// <summary>
    /// Enemy buildings seen at least once: they stay known after the tile
    /// falls back behind the fog.
    ///
    /// Only Contains/Add/Remove in the simulation; the save iterates it sorted
    /// by id, so the set order never enters the computation and two clients
    /// cannot diverge in lockstep. VisibilitySystem clears it on death -
    /// no other writer exists.
    /// </summary>
    public HashSet<int> RememberedEnemyBuildingIds { get; } = [];

    public Player(
        int id,
        string name,
        ConsoleColor color,
        PlayerControllerType controller)
    {
        Id = id;
        Name = name;
        Color = color;
        Controller = controller;

        Economy = new EconomyState();
        Population = new PopulationState();
        AI = new PlayerAIState();
    }

    internal void AddUnit(int unitId) => _unitIds.Add(unitId);
    internal void RemoveUnit(int unitId) => _unitIds.Remove(unitId);
    internal void AddBuilding(int buildingId) => _buildingIds.Add(buildingId);
    internal void RemoveBuilding(int buildingId) => _buildingIds.Remove(buildingId);
}
