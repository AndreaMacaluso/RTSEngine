using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Entities.Definitions;

namespace RTSEngine.Core.Entities.States;
public class GatherState
{
    public int CurrentLoad { get; set; }

    public int Capacity { get; set; }

    public int? TargetResourceId { get; set; }

    public ResourceType? CarriedResource { get; set; }
    public GatherPhase Phase { get; set; }
    public GridPosition? DepositPosition { get; set; }
    public bool IsFull => CurrentLoad >= Capacity;

    public bool IsEmpty => CurrentLoad == 0;

    public int RemainingCapacity => Capacity - CurrentLoad; 
    public GatherState(UnitDefinition definition)
    {
        Capacity = definition.GatherCapacity;
    }

    public int AddLoad(int amount)
    {
        int collected = 0;
        if(!IsFull)
        {
               collected = Math.Min(amount, RemainingCapacity);

               CurrentLoad += collected;

               return collected;
        }
        return collected;
    }

    public void Clear()
    {
        CurrentLoad = 0;
        CarriedResource = null;
    }
}