namespace RTSEngine.Core.Diagnostics;
public sealed class LogScope : IDisposable
{
    private readonly Logger _logger;
    private readonly LogScope? _parent;

    private readonly List<LogScope> _children = [];
    private readonly List<LogEntry> _entries = [];

    public string Name { get; }

    public IReadOnlyList<LogScope> Children => _children;
    public IReadOnlyList<LogEntry> Entries => _entries;

    public LogScope(Logger logger, string name, LogScope? parent = null)
    {
        _logger = logger;
        Name = name;
        _parent = parent;

        _parent?._children.Add(this);
    }
    
    public void Info(
        string message,
        IReadOnlyList<(string Key, object? Value)>? context = null)
    {
        var entry = new LogEntry(LogLevel.Info, message, context);

        _entries.Add(entry);
        _logger.Write(entry);
    }

    public void Warning(string message,
        IReadOnlyList<(string Key, object? Value)>? context = null)
    {
        var entry = new LogEntry(LogLevel.Warning, message, context);

        _entries.Add(entry);
        _logger.Write(entry);
    }

    public LogScope Scope(string name)
    {
        return new LogScope(_logger, name, this);
    }

    public void Dispose()
    {
        // niente obbligatorio per ora
        // (qui potremo fare finalize tree logging in futuro)
    }
}