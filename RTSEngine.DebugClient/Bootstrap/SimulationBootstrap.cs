using RTSEngine.Core.State;
using RTSEngine.Core.Entities.Definitions;

namespace RTSEngine.DebugClient.Bootstrap;

public sealed class SimulationContext
{
    public required GameWorld World { get; init; }

    public required UnitDefinitionRepository UnitRepository { get; init; }

   
    // public required BuildingDefinitionRepository BuildingRepository { get; init; }
}