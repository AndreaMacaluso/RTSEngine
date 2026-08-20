using RTSEngine.Core.Map.Runtime;
namespace RTSEngine.Core.Entities.States;

public sealed class ProductionState
{
    // No queue size limit by design
    private readonly Queue<ProductionTask> _queue = new();
    public GridPosition? SpawnPoint { get; set; }

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