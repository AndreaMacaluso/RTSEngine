namespace RTSEngine.Core.Entities.States;

public sealed class ProductionTask
{
    public string ProductId { get; }
    public int RemainingTicks { get; private set; }
    public ProductionTask(
        string productId,
        int productionTime)
    {
        ProductId = productId;
        RemainingTicks = productionTime;
    }
    public void Tick()
    {
        RemainingTicks--;
    }
    public bool Completed =>
        RemainingTicks <= 0;
}