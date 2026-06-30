
namespace RTSEngine.Core.Players;
public static class PlayerFactory
{
    public static Player Create(int id)
    {
        return new Player
        {
            Id = id
        };
    }
}