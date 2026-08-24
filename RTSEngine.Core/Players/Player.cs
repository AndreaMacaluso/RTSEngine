using RTSEngine.Core.Players.States;
namespace RTSEngine.Core.Players;

public sealed class Player
{
    public int Id { get; }
    public string Name { get; set; } = "";
    public ConsoleColor Color { get; set; }
    public PlayerControllerType Controller { get; set; }
    public int Score { get; set; } = 0;
    public EconomyState Economy { get; }
    public PopulationState Population { get; }
    public PlayerAIState AI { get; }

    private readonly List<int> _unitIds = [];
    private readonly List<int> _buildingIds = [];

    public IReadOnlyList<int> UnitIds => _unitIds;
    public IReadOnlyList<int> BuildingIds => _buildingIds;

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
