namespace RTSEngine.Core.Diagnostics;

public sealed class LogEntry
{
    public DateTime Timestamp { get; }

    public LogLevel Level { get; }

    public string Message { get; }

    public IReadOnlyList<(string Key, object? Value)>? Context { get; }

    public Exception? Exception { get; }
    //public int IndentLevel { get; set; } = 0;

    public LogEntry(
        LogLevel level,
        string message,
        IReadOnlyList<(string Key, object? Value)>? context = null,
        Exception? exception = null)
    {
        Timestamp = DateTime.UtcNow;

        Level = level;
        Message = message;
        Context = context;
        Exception = exception;
    }
}