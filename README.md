# RTSEngine
Tick-based deterministic RTS engine in C# designed for replayability, AI simulation and future Unity integration.

## Design Goals

The simulation core is intentionally renderer-independent.

Rendering, input handling and networking are planned as separate layers in order to preserve deterministic simulation behavior.

## Disclaimer

This project is a personal learning and engineering challenge aimed at building a Real-Time Strategy (RTS) engine from scratch.
It is my first C# project so some of it may seems wrong but its part of the learning process.

Features such as pathfinding, unit management, building construction, resource economy, world state management, and AI are implemented as part of the learning process.
Being my first project in the world of game develpment some parts were developed with AI assistance.

As a result, some solutions may prioritize educational value and architectural clarity over production-level optimization.

## Project Structure

RTSEngine.Core -> deterministic simulation logic

RTSEngine.DebugClient -> debug visualization and runtime testing

RTSEngine.Tests -> unit and integration tests


-----

## Architecture Principles

- Deterministic simulation
- Renderer-independent core
- Tick-based updates
- Test-driven development
- Data-oriented world state

## Roadmap

# Phase 1 — Core Simulation

Goal:
Build a deterministic, renderer-independent RTS simulation core.

---

## World Foundation

- [x] Solution architecture setup
- [x] Engine/client/test project separation
- [x] Initial tile map runtime
- [x] Tile-based grid world
- [x] Grid position abstraction
- [x] Immutable grid position value object
- [x] Tile runtime definitions
- [x] Tile type mapping
- [x] Symbol-based terrain parsing
- [x] JSON-based map definitions
- [x] Runtime tile map builder
- [x] Data-driven map loading pipeline
- [x] Runtime asset loading pipeline
- [x] Map validation system
- [x] Terrain properties system
- [x] Walkable tile rules
- [x] Buildable tile rules
- [x] Tile occupancy system
- [x] Runtime entity placement validation
- [x] World state management
- [x] Data-driven map units pipeline

---

## Simulation Core

- [x] Basic simulation loop
- [x] Simulation runner
- [x] Runtime world container
- [x] Runtime entity registration
- [x] Runtime entity id generation
- [x] Basic entity runtime
- [x] Runtime movement state
- [x] Movement progress accumulation
- [x] Fixed deterministic tick pipeline
- [x] Entity movement system
- [x] Multi-step movement pipeline
- [x] Command queue system
- [x] Command dispatch pipeline
- [x] State management layer

---

## Map System

- [x] JSON map loading
- [x] Terrain deserialization
- [x] Resource deserialization
- [x] Spawn deserialization
- [x] Resource runtime entities
- [x] Resource factory
- [x] Tree resource node
- [x] Gold mine resource node
- [x] Stone mine resource node
- [x] Berry bush resource node
- [x] Spawn point definitions
- [x] 40x40 debug map support
- [x] Resource placement validation
- [x] Spawn validation rules
- [x] Procedural map generation
- [x] Symmetric procedural map generation

---

## Rendering

- [x] Console renderer
- [x] Colored terrain rendering
- [x] Colored resource rendering
- [x] Colored spawn rendering
- [x] Minimal render mode
- [x] Extended render mode
- [x] UTF-8 symbol rendering
- [x] Runtime unit rendering
- [x] Runtime debug controls
- [x] Tick pause system
- [x] Debug overlays
- [x] Unit creation logging (id + type)

---
## Command System

- [x] Basic command queue system
- [ ] Advanced command scheduling

---

## Movement

- [x] 8-direction tile movement
- [x] Tile occupancy validation
- [x] Adjacent tile validation
- [x] Terrain collision validation
- [x] Entity collision validation
- [x] Deterministic movement progression
- [x] Multi-step path movement
- [x] Path queue execution
- [x] Basic pathfinding foundations
- [x] Dynamic collision handling

### Pathfinding
- [x] Basic path generation pipeline
- [x] Command-to-path integration
- [x] A* pathfinding with Octile heuristic
- [x] Path reconstruction from search
- [x] Unreachable target handling
- [x] Dynamic collision handling
- [x] Path replanning / blocked path recovery
---

### Trigger System

- [x] Data-driven trigger loading from JSON
- [x] MissionLoader (PropertyNameCaseInsensitive + JsonStringEnumConverter)
- [x] Condition types: OwnObjects, Timer (with StartTick)
- [x] Effect types: CreateObject, SendChat, TaskMovementObject, AttackMove
- [x] TriggerHandler — condition-driven evaluation (AND logic)
- [x] TriggerFactory / ConditionFactory / EffectFactory
- [x] A* blocked target handling (nearest walkable fallback)
- [ ] More condition types (PlayerDefeated, ObjectsInArea)
- [ ] More effect types (DeclareVictory)
- [ ] Event-driven triggers (via EventBus)

---

## Data-Driven Entities

- [x] Unit definitions from JSON
- [x] Unit definition loader
- [x] Unit definition repository
- [x] Unit factory
- [x] Building definitions from JSON
- [x] Building definition loader
- [ ] Runtime unit state model
- [ ] Runtime building state model

---

## Testing

- [x] Unit testing infrastructure
- [x] Tile map validation tests
- [x] JSON map loading tests
- [x] Tile type mapping tests
- [x] Resource definition tests
- [x] Builder validation tests
- [x] Terrain rule validation tests
- [x] Movement system tests
- [x] Occupancy validation tests
- [x] Adjacent movement validation tests
- [x] Queued movement tests
- [x] Command system tests
- [x] Tick determinism tests
- [x] Resource runtime tests
- [x] Pathfinding tests
- [x] Gather command tests
- [x] Gather actions tests
- [x] Gather system tests
- [x] Construction actions tests
- [x] Construction system tests
- [x] Building placement tests
- [x] Economy actions tests
- [x] Resource cleanup tests
- [x] End-to-end gather tests
- [x] End-to-end construction tests
- [x] Gather decision tests
- [x] Gather AI actions tests
- [x] Construction decision tests
- [x] Construction AI actions tests
- [x] Building planner tests
- [x] AI system tests
- [x] Combat AI tests
- [x] Militia combat AI tests
- [x] Barracks AI tests
- [x] Production state tests
- [x] Production action tests
- [x] Production system tests
- [x] Production command tests
- [x] End-to-end unit production tests
- [x] Projectile system tests
- [x] Score system tests
- [x] Victory state tests
- [x] Watch tower tests
- [x] Ground attack tests
- [x] Guard and stop tests
- [x] Rally point tests
- [x] Unit queries tests
- [x] World queries tests
- [x] Spatial index tests
- [x] Symmetric map generator tests

## Debug Client

- [x] Simulation bootstrap
- [x] Runtime simulation host
- [x] World initialization pipeline
- [x] Movement demonstration scenario
- [x] AI player integration
- [x] Scenario selection
- [ ] Interactive unit selection
- [ ] Runtime command issuing

---

# Phase 2 — Gameplay Systems

## Game Loop Core

- Economy Loop
- Construction Loop
- Production Loop
- Combat Loop
- Trade Loop

---

## Game Loop Details

### Economy

- [x] Gather command
- [x] Resource targeting loop
- [x] Unit gather runtime state
- [x] Gather execution system
- [x] Carry capacity loop
- [x] Deposit / drop-off loop
- [x] Resource retargeting
- [x] Continuous gathering
- [x] Resource cleanup
- [x] Gather state machine
- [x] End-to-end villager gather cycle
- [x] Gather interruption handling
- [x] Dynamic deposit selection
- [x] Resource stockpile
- [x] Resource payment
- [x] Multiple resource gathering
- [x] Resource depletion cleanup
- [ ] Search radius
- [ ] Resource balancing

### Construction

- [x] Build command
- [x] Foundation placement
- [x] Move to construction
- [x] Construction state machine
- [x] Construction progress
- [x] Completion
- [x] Rendering
- [x] Multi-tile structures
- [x] Cancel construction
- [x] Building cancellation / refund
- [ ] Multiple builders
- [ ] Repair system
- [ ] Drop-off buildings
- [ ] Garrison system

### Production

- [x] Building production queue
- [x] Unit training command
- [x] Training progress system
- [x] Unit spawn from building
- [x] End-to-end production cycle
- [x] Resource payment validation
- [x] Resource payment on production command
- [x] Production cancellation / refund
- [ ] Parallel build queue (multiple buildings)

### Combat

- [x] Target selection system
- [x] Attack cooldown system
- [x] Melee combat
- [x] Ranged combat
- [x] Chase behavior
- [x] Stop on target death
- [x] Health and damage
- [x] Projectile system (single-target + splash)
- [x] Damage calculator with category bonuses
- [x] Combat decision (AI auto-attack)
- [x] Enemy building targeting
- [x] Militia base-attack AI (move toward enemy TC)
- [x] All idle military units act
- [x] Defensive buildings (Watch Tower)
- [x] Building combat (tower defense)
- [ ] Formation system
- [ ] Guard behavior
- [ ] Patrol loop

### Trade

- [ ] Trade route system
- [ ] Merchant units
- [ ] Resource exchange
- [ ] Trade income
- [ ] Trade route protection

---

## Gameplay Systems

### Population

- [x] Player economy
- [x] Population
- [x] Population cap
- [x] Population management (reserve/release)
- [x] AI players

### Score & Victory

- [x] Score system
- [x] Victory condition (Conquest + Score Limit)

### Vision

- [ ] Fog of war
- [ ] Vision memory
- [ ] Visibility updates

### Progression

- [ ] Tech tree

---

# Phase 3 — AI Systems

> **Note:** Current AI is rule-based scaffolding to validate game mechanics.
> Will be replaced by Lua scripting in Phase 7. Treat this code as temporary/disposable.

## Core

- [x] AI player controller
- [x] AI update system
- [x] AI decision interval
- [x] Runtime AI state
- [ ] AI command system

## Decisions

- [x] Gather decision
- [x] Construction decision
- [x] Production decision
- [x] Combat decision (auto-attack idle military)
- [ ] Exploration decision
- [ ] Economy management AI
- [ ] Scout system

## Actions

- [x] Gather AI actions
- [x] Construction AI actions
- [x] Production AI actions
- [x] Combat AI actions
- [x] Barracks construction decision (pop >= 15)
- [x] Militia training from barracks
- [ ] Build order execution
- [ ] Reactive AI behaviors

---

# Phase 4 — Tooling & Integration

- [ ] Replay system
- [ ] Save/load system
- [ ] Map editor
- [x] Colored debug visualization
- [x] Runtime debug controls
- [x] Debug visualization improvements
- [ ] Unity integration layer
- [ ] GUI (non-console renderer)

---

# Phase 5 — Architecture Rework (in progress)

Goal: clean up architecture, fix known issues, prepare for Lua integration.

### Pathfinding Refactoring
- [x] Extract pathfinding behind interface (`IPathFinder`)
- [x] Object pooling for path allocations
- [x] Spatial index for O(1) position lookups
- [x] Decouple CommandQueue from GameWorld (`ICommandQueue` in RuntimeContext)
- [x] A* pathfinding with Octile heuristic
- [x] IMovementFilter for extensible passability checks
- [x] SpatialIndex — handle multiple units on same tile
- [x] SpatialIndex — double rebuild fix
- [x] SpatialIndex — ToList allocations removed
- [x] SpatialIndex — IsTileBlocked checks all units
- [x] SpatialIndex — dead units filtered on rebuild
- [ ] Add entity validation to AddEntity/RemoveEntity

### Bug Fixes
- [x] ConstructionSystem — repath loop infinite (BuildOneTick return + IsCompleted guard)
- [x] ConstructionSystem — buildings removed during construction (FindDeadBuildings filter)
- [x] Entity.TakeDamage — reject negative values
- [x] CommandSystem — add OwnerId validation
- [x] MovementSystem — step lost when blocked
- [x] Building.IsDead — does not affect construction (verified)
- [x] ConstructionSystem — CompleteConstruction releases builder
- [x] ConstructionSystem — BuildPosition redundant field removed
- [x] TileType — Forest mapped in TileTypeMapper
- [x] ProductionSystem — .ToList() allocation removed
- [x] Trigger system — TriggerHandler ordering (moved before CommandSystem)
- [x] A* pathfinding — blocked target handling (finds nearest walkable tile)
- [x] MovementSystem — Attacking units with empty path go to Idle
- [x] MovementSystem — NeedRepath extended to Attacking units
- [x] CreateObjectEffect — spawn position validation (uses FindAdjacentWalkableTile)
- [x] AttackMoveEffect — filter military units only (uses UnitQueries.FindIdleMilitary)
- [x] TimerCondition — added StartTick field
- [x] EffectType — removed unimplemented types
- [x] TaskObjectEffect — renamed to TaskMovementObjectEffect
- [ ] CombatSystem — re-path when target moves out of range (deferred)
- [ ] BasicAi — brain instantiation optimization (deferred)

### Code Quality
- [x] Fix typos: CreeateGatheringScenario, ComandSystemTest, ResurceNodeTest
- [x] Fix file/class mismatches: BuildingState.cs, BuildingPhase.cs, BasicAi.cs, EconomicActions.cs, GatherAction.cs, ResourceCleanUpSystem.cs
- [x] Fix abstract public → public abstract
- [x] Add sealed to Unit class
- [x] Remove debug logging from GatherSystem
- [x] File naming: ResourceCleanUpSystem, AiSystem, BasicAi
- [x] Namespace: Loader → Loaders
- [x] UnitDefinitionRepository — sealed added
- [x] Double space in usings removed
- [x] GatherActions — UnitIds parameter to camelCase
- [x] MovementState — PathQueue setter removed (get-only)
- [x] Building — PopulationBonus dead field removed
- [x] LogScope — IDisposable removed
- [x] IsTileBlocked — null tile handling
- [x] TriggerSystemTests — updated to project standards
- [ ] Rename remaining test files (Test → Tests suffix)
- [ ] Clean up hardcoded config values (move to settings)

### Duplicated Logic
- [x] GroundMovementFilter.CanPass duplicated IsTileBlocked
- [x] TrainUnit duplicated TryTrainUnit validations
- [x] FindClosestResource two identical overloads
- [x] DefinitionLoader duplicated (Unit/Building)

---

# Phase 6 — Core Systems

Goal: complete core infrastructure before plugins.

## Event System

- [ ] Event bus (publish/subscribe)
- [ ] Event types (UnitSpawned, BuildingCompleted, etc.)
- [ ] Event registry (event → trigger mapping)
- [ ] Trigger interface (Evaluate + Execute)

## Determinism

- [ ] Fixed-point math verification
- [ ] Seeded RNG
- [ ] Determinism test suite

---

# Phase 7 — Lua Scripting (future)

Goal: replace hardcoded AI with data-driven Lua scripts.

- [ ] Embed Lua runtime
- [ ] Create scripting API (read world, issue commands)
- [ ] Migrate AI brains to Lua
- [ ] Event system for Lua callbacks
- [ ] Test AI scripts

---

# Phase 8 — Multiplayer

Goal: online multiplayer with lockstep synchronization.

## Networking Core

- [ ] Network manager
- [ ] Lockstep sync
- [ ] Host/Client architecture

## Command Sync

- [ ] Command serializer
- [ ] Command log per tick
- [ ] Server-side validation

---
