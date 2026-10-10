namespace RTSEngine.Core.Commands;

public class StopCommand : ICommand
{
    public required List<int> UnitIds { get; init; }

    public required int PlayerId { get; init; }
}
