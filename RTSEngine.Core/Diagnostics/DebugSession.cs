namespace RTSEngine.Core.Diagnostics;

public static class DebugSession
{
    public static Logger Log { get; private set; } = new();

    public static bool IsInitialized { get; private set; }

    public static void Initialize(Logger logger)
    {
        Log = logger;
        IsInitialized = true;
    }
}