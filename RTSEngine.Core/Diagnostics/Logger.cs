namespace RTSEngine.Core.Diagnostics;

public sealed class Logger
{
    private readonly List<ILogSink> _sinks = [];

    public void AddSink(ILogSink sink)
    {
        _sinks.Add(sink);
    }

    public void Info(
        string message,
        IReadOnlyList<(string Key, object? Value)>? context = null,
        Exception? exception = null)
    {
        Write(new LogEntry(
            LogLevel.Info,
            message,
            context,
            exception));
    }

    public void Warning(
       string message,
        IReadOnlyList<(string Key, object? Value)>? context = null,
        Exception? exception = null)
    {
        Write(new LogEntry(
            LogLevel.Warning,
            message,
            context,
            exception));
    }

    public void Error(
        string message,
        IReadOnlyList<(string Key, object? Value)>? context = null,
        Exception? exception = null)
    {
        Write(new LogEntry(
            LogLevel.Error,
            message,
            context,
            exception));
    }

    public void Write(LogEntry entry)
    {
        foreach (var sink in _sinks)
        {
            sink.Write(entry);
        }
    }
    public LogScope Scope(string name)
    {
        return new LogScope(this, name);
    }
}