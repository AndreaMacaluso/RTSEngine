namespace RTSEngine.Core.Map.Definitions;

public class MapData
{
    public int Width { get; set; }

    public int Height { get; set; }

    public List<string> Rows { get; set; } = new();
}