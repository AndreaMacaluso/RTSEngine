namespace RTSEngine.Core.Settings;

public sealed class EngineSettings
{
    public int TickRate { get; init; } = 16;
    public int DecayTicks { get; init; } = 5;
    public bool DebugMode { get; init; } = false;
}
