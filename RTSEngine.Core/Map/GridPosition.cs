namespace RTSEngine.Core.Map;
public struct GridPosition
{
    public int X { get; set; }

    public int Y { get; set; }

    public GridPosition(int x, int y)
    {
        X = x;
        Y = y;
    }
}