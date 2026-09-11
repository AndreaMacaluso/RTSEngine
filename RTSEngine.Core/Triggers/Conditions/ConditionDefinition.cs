namespace RTSEngine.Core.Triggers;

public sealed class ConditionDefinition
{
    public int ConditionType { get; set; }
    public int? PlayerId { get; set; }
    public int? ObjectId { get; set; }
    public string? ObjectTypeId { get; set; }
    public int? Quantity { get; set; }
    public int? AreaX1 { get; set; }
    public int? AreaY1 { get; set; }
    public int? AreaX2 { get; set; }
    public int? AreaY2 { get; set; }
    public int? Time { get; set; }
}
