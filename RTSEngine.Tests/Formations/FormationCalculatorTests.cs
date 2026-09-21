using RTSEngine.Core.Commands;
using RTSEngine.Core.Map.Runtime;
using Xunit;

namespace RTSEngine.Tests.Formations;

[Trait("Category", "Formations")]
public class FormationCalculatorTests
{
    [Fact]
    public void Line_ShouldReturnCorrectPositions_WhenMultipleUnits()
    {
        var center = new GridPosition(20, 20);
        var positions = FormationCalculator.Calculate(
            FormationType.Line, 5, center);

        Assert.Equal(5, positions.Count);
        Assert.Equal(new GridPosition(18, 20), positions[0]);
        Assert.Equal(new GridPosition(19, 20), positions[1]);
        Assert.Equal(new GridPosition(20, 20), positions[2]);
        Assert.Equal(new GridPosition(21, 20), positions[3]);
        Assert.Equal(new GridPosition(22, 20), positions[4]);
    }

    [Fact]
    public void Line_ShouldReturnCenter_WhenSingleUnit()
    {
        var center = new GridPosition(20, 20);
        var positions = FormationCalculator.Calculate(
            FormationType.Line, 1, center);

        Assert.Single(positions);
        Assert.Equal(center, positions[0]);
    }

    [Fact]
    public void Box_ShouldReturnCorrectPositions_WhenMultipleUnits()
    {
        var center = new GridPosition(20, 20);
        var positions = FormationCalculator.Calculate(
            FormationType.Box, 4, center);

        Assert.Equal(4, positions.Count);
        Assert.All(positions, p =>
        {
            Assert.True(Math.Abs(p.X - center.X) <= 1);
            Assert.True(Math.Abs(p.Y - center.Y) <= 1);
        });
    }

    [Fact]
    public void Box_ShouldReturnCenter_WhenSingleUnit()
    {
        var center = new GridPosition(20, 20);
        var positions = FormationCalculator.Calculate(
            FormationType.Box, 1, center);

        Assert.Single(positions);
        Assert.Equal(center, positions[0]);
    }

    [Fact]
    public void Staggered_ShouldReturnCorrectCount_WhenMultipleUnits()
    {
        var center = new GridPosition(20, 20);
        var positions = FormationCalculator.Calculate(
            FormationType.Staggered, 6, center);

        Assert.Equal(6, positions.Count);
    }

    [Fact]
    public void Staggered_ShouldHaveSpacing_WhenMultipleUnits()
    {
        var center = new GridPosition(20, 20);
        var positions = FormationCalculator.Calculate(
            FormationType.Staggered, 4, center);

        Assert.Equal(4, positions.Count);
        var uniquePositions = positions.Distinct().ToList();
        Assert.Equal(4, uniquePositions.Count);
    }

    [Fact]
    public void None_ShouldReturnSamePositionForAll_WhenNoFormation()
    {
        var center = new GridPosition(20, 20);
        var positions = FormationCalculator.Calculate(
            FormationType.None, 5, center);

        Assert.Equal(5, positions.Count);
        Assert.All(positions, p => Assert.Equal(center, p));
    }

    [Fact]
    public void Line_ShouldReturnCorrectPositions_WhenEvenCount()
    {
        var center = new GridPosition(20, 20);
        var positions = FormationCalculator.Calculate(
            FormationType.Line, 4, center);

        Assert.Equal(4, positions.Count);
        Assert.Equal(new GridPosition(18, 20), positions[0]);
        Assert.Equal(new GridPosition(19, 20), positions[1]);
        Assert.Equal(new GridPosition(20, 20), positions[2]);
        Assert.Equal(new GridPosition(21, 20), positions[3]);
    }

    [Fact]
    public void Box_ShouldReturnSquareGrid_WhenManyUnits()
    {
        var center = new GridPosition(20, 20);
        var positions = FormationCalculator.Calculate(
            FormationType.Box, 9, center);

        Assert.Equal(9, positions.Count);
        var uniquePositions = positions.Distinct().ToList();
        Assert.Equal(9, uniquePositions.Count);
    }
}
