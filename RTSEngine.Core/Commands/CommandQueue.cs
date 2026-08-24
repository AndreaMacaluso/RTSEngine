namespace RTSEngine.Core.Commands;

public class CommandQueue : ICommandQueue
{
    private readonly Queue<ICommand> _queue = new();

    public void Enqueue(ICommand command)
    {
        _queue.Enqueue(command);
    }

    public ICommand? Dequeue()
    {
        return _queue.Count > 0
            ? _queue.Dequeue()
            : null;
    }

    public int Count => _queue.Count;

    public IReadOnlyCollection<ICommand> Pending => _queue;
}
