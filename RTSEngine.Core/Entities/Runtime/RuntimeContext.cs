using RTSEngine.Core.Commands;
using RTSEngine.Core.State;
using RTSEngine.Core.Entities.Definitions;
using RTSEngine.Core.Systems.Pathfinding;
using RTSEngine.Core.Settings;
using RTSEngine.Core.Triggers;

namespace RTSEngine.Core.Entities.Runtime;

public sealed class RuntimeContext
{
    //@ToDo VictoryState ProjectileState move in game world or other class
    //VictoryState is a game state rework in game world state machine
    //PathFinder rewiew patfinder if needed there or can be in another class
    public required GameWorld World { get; init; }

    public required UnitDefinitionRepository UnitRepository { get; init; }
  
    public required BuildingDefinitionRepository BuildingRepository { get; init; }

    public required ICommandQueue CommandQueue { get; init; }

    public required IPathFinder PathFinder { get; init; }

    public required GameSettings Settings { get; init; }

    public TriggerHandler TriggerHandler { get; init; } = new();

    public VictoryState Victory { get; } = new();

    public ProjectileState Projectiles { get; } = new();
}