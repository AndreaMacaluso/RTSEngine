using RTSEngine.Core.Map.Runtime;

namespace RTSEngine.Core.Commands;

public static class FormationCalculator
{
    public static List<GridPosition> Calculate(
        FormationType type,
        int unitCount,
        GridPosition center)
    {
        return type switch
        {
            FormationType.Line => CalculateLine(unitCount, center),
            FormationType.Box => CalculateBox(unitCount, center),
            FormationType.Staggered => CalculateStaggered(unitCount, center),
            _ => CalculateDefault(unitCount, center)
        };
    }

    private static List<GridPosition> CalculateLine(
        int unitCount,
        GridPosition center)
    {
        var positions = new List<GridPosition>(unitCount);
        int startX = center.X - unitCount / 2;

        for (int i = 0; i < unitCount; i++)
        {
            positions.Add(new GridPosition(startX + i, center.Y));
        }

        return positions;
    }

    private static List<GridPosition> CalculateBox(
        int unitCount,
        GridPosition center)
    {
        var positions = new List<GridPosition>(unitCount);

        if (unitCount == 1)
        {
            positions.Add(center);
            return positions;
        }

        int side = (int)Math.Ceiling(Math.Sqrt(unitCount));
        int halfSide = side / 2;

        for (int i = 0; i < unitCount; i++)
        {
            int row = i / side;
            int col = i % side;
            int x = center.X - halfSide + col;
            int y = center.Y - halfSide + row;
            positions.Add(new GridPosition(x, y));
        }

        return positions;
    }

    private static List<GridPosition> CalculateStaggered(
        int unitCount,
        GridPosition center)
    {
        var positions = new List<GridPosition>(unitCount);
        int spacing = 2;
        int placed = 0;
        int row = 0;

        while (placed < unitCount)
        {
            int unitsInRow = Math.Min(3, unitCount - placed);
            int startX = center.X - (unitsInRow - 1) * spacing / 2;

            for (int i = 0; i < unitsInRow; i++)
            {
                int offsetX = (row % 2 == 1) ? spacing / 2 : 0;
                positions.Add(new GridPosition(
                    startX + i * spacing + offsetX,
                    center.Y + row * spacing));
                placed++;
            }

            row++;
        }

        return positions;
    }

    private static List<GridPosition> CalculateDefault(
        int unitCount,
        GridPosition center)
    {
        var positions = new List<GridPosition>(unitCount);

        for (int i = 0; i < unitCount; i++)
        {
            positions.Add(center);
        }

        return positions;
    }
}
