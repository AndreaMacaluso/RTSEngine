namespace RTSEngine.Core.Commands;

public class StopCommand : ICommand
{
    public required List<int> UnitIds { get; init; }
}
