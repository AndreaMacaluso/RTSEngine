namespace RTSEngine.Core.Entities.Units;

public abstract class Unit : Entity
{
    public override bool IsBlocking => true; 
     public int OwnerId { get; init; }
}