using RTSEngine.Core.Map.Runtime;

namespace RTSEngine.Core.Commands;

public class ProductionCommand : ICommand
{
    public int PlayerId { get; init; }
    public int BuildingId { get; init; }
    public ProductionActionType Action { get; init; }
    public string? ProductId { get; init; }
    public GridPosition? Target { get; init; }
}
