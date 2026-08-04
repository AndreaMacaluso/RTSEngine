using RTSEngine.Core.Players.States;
namespace RTSEngine.Core.Players;

public sealed class Player
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public ConsoleColor Color { get; set; }
    public PlayerControllerType Controller { get; set; }
    public EconomyState Economy { get; }
    public PopulationState Population { get; }
    public PlayerAIState AI { get; }
    public Player(
        int id,
        string name,
        ConsoleColor color,
        PlayerControllerType controller)
    {
        Id = id;
        Name = name;
        Color = color;
        Controller = controller;

        Economy = new EconomyState();

        Population = new PopulationState();

        //AI = new PlayerAIState();
    }

    public Player()
    {
        Economy = new EconomyState();

        Population = new PopulationState();

        //AI = new PlayerAIState();
    }
}