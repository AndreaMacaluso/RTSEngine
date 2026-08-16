namespace RTSEngine.Core.Commands;

public class AttackCommand : ICommand
{
    public List<int> UnitIds { get; set; } = [];

    public int TargetEntityId { get; set; }
}
