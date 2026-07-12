namespace RTSEngine.Core.Commands;
public class BuildCommand : ICommand
{
    public required List<int> UnitIds { get; init; }

    public required int BuildingId { get; init; }
}