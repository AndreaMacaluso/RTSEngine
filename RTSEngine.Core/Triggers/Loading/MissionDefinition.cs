namespace RTSEngine.Core.Triggers;

public sealed class MissionDefinition
{
    public string MissionId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<TriggerDefinition> Triggers { get; set; } = new();
}
