namespace RTSEngine.Core.Diagnostics;

public sealed class Logger
{
    private readonly List<ILogSink> _sinks = [];
    private readonly bool _debugEnabled;

    public Logger(bool debugEnabled = false)
    {
        _debugEnabled = debugEnabled;
    }

    public void AddSink(ILogSink sink)
    {
        _sinks.Add(sink);
    }

    public void Debug(
        string message,
        IReadOnlyList<(string Key, object? Value)>? context = null,
        Exception? exception = null)
    {
        if (!_debugEnabled)
        {
            return;
        }

        Write(new LogEntry(
            LogLevel.Debug,
            message,
            context,
            exception));
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