namespace RTSEngine.Core.Diagnostics;
public sealed class FileLogSink : ILogSink, IDisposable
{
    private readonly StreamWriter _writer;
    private int _indent = 0;

    public FileLogSink(string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        _writer = new StreamWriter(path, append: false);
    }

    public void Write(LogEntry entry)
    {
        if (entry.Message.StartsWith("MovementSystem") ||
            entry.Message.StartsWith("Scenario"))
        {
            _indent = 0;
        }

        var indent = new string(' ', _indent * 2);

        _writer.WriteLine($"{indent}[{entry.Level}] {entry.Message}");

        if (entry.Context != null)
        {
            _indent++;

            foreach (var (key, value) in entry.Context)
            {
                var ctxIndent = new string(' ', _indent * 2);
                string text = value?.ToString() ?? "<null>";
                _writer.WriteLine($"{ctxIndent}{key}: {text}");
            }

            _indent--;
        }
        _writer.Flush();
    }

    public void Dispose()
    {
        _writer.Flush();
        _writer.Dispose();
    }
}