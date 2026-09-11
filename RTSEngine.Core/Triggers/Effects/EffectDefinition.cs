namespace RTSEngine.Core.Triggers;

public sealed class EffectDefinition
{
    public int EffectType { get; set; }
    public int? PlayerId { get; set; }
    public string? ObjectTypeId { get; set; }
    public int? LocationX { get; set; }
    public int? LocationY { get; set; }
    public string? Message { get; set; }
    public int? TriggerId { get; set; }
}
