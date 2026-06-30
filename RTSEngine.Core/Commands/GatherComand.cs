namespace RTSEngine.Core.Commands;
public class GatherCommand : ICommand
{
    public required List<int> UnitIds { get; init; }

    public required int ResourceId { get; init; }
}