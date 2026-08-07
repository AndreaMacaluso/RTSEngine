using RTSEngine.Core.Map.Runtime;
namespace RTSEngine.Core.Entities.States;

public sealed class ProductionState
{
    private readonly Queue<ProductionTask> _queue = new();
    public GridPosition? SpawnPoint { get; set; }

    // new GridPosition(
    // building.Position.X + building.Definition.Width,
    // building.Position.Y)
    public bool IsProducing => Current != null;
    public ProductionTask? Current =>
        _queue.Count > 0
        ? _queue.Peek()
        : null;


    public void Add(
        ProductionTask task)
    {
        _queue.Enqueue(task);
    }


    public void RemoveCurrent()
    {
        if(_queue.Count > 0)
            _queue.Dequeue();
    }
}