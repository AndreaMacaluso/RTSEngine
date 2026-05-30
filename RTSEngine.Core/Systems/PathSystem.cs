using RTSEngine.Core.Map.Runtime;

namespace RTSEngine.Core.Systems;

public static class PathSystem
{
    public static Queue<GridPosition> GeneratePath(
        GridPosition start,
        GridPosition target)
    {
        var path = new Queue<GridPosition>();

        var current = start;

        while (current != target)
        {
            int dx = Math.Sign(target.X - current.X);
            int dy = Math.Sign(target.Y - current.Y);

            current = new GridPosition(
                current.X + dx,
                current.Y + dy);

            path.Enqueue(current);
        }

        return path;
    }
}