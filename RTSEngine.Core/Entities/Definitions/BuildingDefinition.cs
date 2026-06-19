public class BuildingDefinition
{
    public string Id { get; set; }

    public string Name { get; set; }

    public int MaxHealth { get; set; }

    public int Width { get; set; }

    public int Height { get; set; }

    public int BuildTimeTicks {get; set;}

    public Dictionary<string, int> Costs { get; set; } = [];
}