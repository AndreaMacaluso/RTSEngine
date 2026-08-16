namespace RTSEngine.Core.Entities.States;
public enum GatherPhase
{  
    None,
    MovingToResource,
    Gathering,
    MovingToDeposit,
    WaitingForDeposit,
    Depositing
}