using RTSEngine.Core.Commands;
using RTSEngine.Core.State;
using RTSEngine.Core.Entities.Definitions;

namespace RTSEngine.Core.Entities.Runtime;

public sealed class RuntimeContext
{
    public required GameWorld World { get; init; }

    public required UnitDefinitionRepository UnitRepository { get; init; }
  
    public required BuildingDefinitionRepository BuildingRepository { get; init; }

    public required ICommandQueue CommandQueue { get; init; }
}