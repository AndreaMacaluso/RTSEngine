using RTSEngine.Core.Map.Runtime;

namespace RTSEngine.Core.Entities.States;
public class GatherState
{
    public int CurrentLoad { get; set; }

    public int Capacity { get; set; }

    public int? TargetResourceId { get; set; }

    public ResourceType? CarriedResource { get; set; }
    public bool IsFull => CurrentLoad >= Capacity;

    public bool IsEmpty => CurrentLoad == 0;

    public int RemainingCapacity => Capacity - CurrentLoad; 
    public GatherState(UnitDefinition definition)
    {
        Capacity = definition.GatherCapacity;
    }

    public int AddLoad(int amount)
    {
        int collected = amount;
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