namespace RTSEngine.Core.Commands;
public sealed class QueueProductionCommand : ICommand
{
    public int PlayerId {get;}

    public int BuildingId {get;}

    public string ProductId {get;}


    public QueueProductionCommand(
        int playerId,
        int buildingId,
        string productId)
    {
        PlayerId = playerId;
        BuildingId = buildingId;
        ProductId = productId;
    }
}