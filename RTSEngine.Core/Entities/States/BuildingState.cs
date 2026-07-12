using RTSEngine.Core.Map.Runtime;

namespace RTSEngine.Core.Entities.States;

public class BuildState
{
    public int? BuildingId { get; set; }

    public BuildPhase Phase { get; set; }

    public GridPosition? BuildPosition { get; set; }

    public bool IsBuilding => Phase != BuildPhase.None;

    public BuildState()
    {
        Phase = BuildPhase.None;
    }
}