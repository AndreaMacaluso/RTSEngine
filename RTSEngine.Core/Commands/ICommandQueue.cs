namespace RTSEngine.Core.Commands;

public interface ICommandQueue
{
    void Enqueue(ICommand command);
    ICommand? Dequeue();
    int Count { get; }
    IReadOnlyCollection<ICommand> Pending { get; }
}
