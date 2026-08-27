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
RTSEngine.Core          
├── Actions -> atomic state mutations
├── AI -> brain system (temporary, will be replaced by Lua)
├── Commands -> command pattern
├── Diagnostics -> logging framework
├── Entities -> entity hierarchy (Units, Buildings, Resources)
├── Helpers -> query helpers (WorldQueries, UnitQueries)
├── Map -> tile map, generation, loading
├── Players -> player + states (Economy, Population)
├── Simulation -> simulation runner
├── Systems -> game systems (Movement, Gather, Combat, etc.)
└── State -> GameWorld, WorldState
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
- [x] True BFS pathfinding
- [x] Path reconstruction from BFS search
- [x] Unreachable target handling
- [x] Dynamic collision handling
- [x] Path replanning / blocked path recovery

---
## Gameplay Loops

### Economy Loop

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

### Production Loop

- [x] Building production queue
- [x] Unit training command
- [x] Training progress system
- [x] Unit spawn from building
- [x] End-to-end production cycle
- [x] Resource payment validation
- [x] Resource payment on production command
- [ ] Production cancellation / refund

### Construction Loop

- [x] Build command
- [x] Foundation
- [x] Move to construction
- [x] Construction state machine
- [x] Construction progress
- [x] Completion
- [x] Rendering

- [ ] Multiple builders
- [ ] Repair
- [x] Cancel construction

## Runtime Gameplay Loop

- [x] Initial economy
- [x] Initial town center
- [x] Villager spawn
- [x] Gather loop
- [x] Construction loop
- [x] Production loop
- [x] Combat loop
- [x] Military unit production
- [x] Barracks AI integration
- [x] Building destruction

## World Queries

- [x] Adjacent tile queries
- [x] Adjacent walkable tile search
- [x] Closest adjacent walkable tile
- [x] Closest resource search
- [x] Nearby resources search
- [x] Closest deposit search

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

## Debug Client

- [x] Simulation bootstrap
- [x] Runtime simulation host
- [x] World initialization pipeline
- [x] Movement demonstration scenario
- [x] AI player integration
- [ ] Interactive unit selection
- [ ] Runtime command issuing
- [ ] Scenario selection

---

# Phase 2 — Gameplay Systems

## Economy

- [x] Resource stockpile
- [x] Resource payment
- [x] Gathering
- [x] Deposit
- [x] Automatic retargeting
- [x] Multiple resource gathering

- [x] Resource depletion cleanup
- [ ] Search radius
- [x] Gather interruption
- [x] Dynamic deposit selection
- [ ] Resource balancing

---

## Buildings

- [x] Runtime building entities
- [x] Town Center
- [x] Building factory
- [x] Foundation placement
- [x] Building placement validation
- [x] Resource payment
- [x] Tile occupation
- [x] Build command
- [x] Construction state machine
- [x] Construction progress
- [x] Building completion
- [x] Barracks

- [x] Multi-tile structures
- [x] Building cancellation
- [x] Building refund
- [ ] Repair system
- [x] Building destruction
- [x] Production buildings
- [ ] Drop-off buildings

---

## Combat

- [x] Combat system
- [x] Health and damage
- [x] Attack cooldown system
- [x] Target selection system
- [x] Melee combat
- [x] Combat chase behavior
- [x] Combat stop on target death
- [x] Combat decision (AI auto-attack)
- [x] Enemy building targeting
- [x] Militia base-attack AI (move toward enemy TC)
- [x] All idle military units act

---

## Vision

- [ ] Fog of war
- [ ] Vision memory
- [ ] Visibility updates


## Gameplay

- [x] Player economy
- [x] Population
- [x] Population cap
- [x] AI players

- [x] Unit production
- [x] Militia production
- [ ] Tech tree

---

# Phase 3 — AI Systems

The current AI uses a rule-based brain system. This is a temporary implementation to validate game mechanics. 
The AI will be rewritten in Lua scripting once the architecture is stable.

- [x] AI player controller
- [x] AI update system
- [x] AI decision interval
- [x] Runtime AI state
- [x] Gather decision
- [x] Construction decision
- [x] Gather AI actions
- [x] Construction AI actions
- [x] Production decision
- [ ] Exploration decision
- [x] Production AI actions
- [x] Combat AI actions
- [x] Combat decision (auto-attack idle military)
- [x] Barracks construction decision (pop >= 15)
- [x] Militia training from barracks
- [ ] AI command system
- [ ] Build order execution
- [ ] Reactive AI behaviors
- [ ] Scout system
- [ ] Economy management AI

---

# Phase 4 — Tooling & Integration

- [ ] Replay system
- [ ] Save/load system
- [ ] Map editor
- [x] Colored debug visualization
- [x] Runtime debug controls
- [ ] Debug visualization improvements
- [ ] Unity integration layer

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
- [ ] Rename remaining test files (Test → Tests suffix)
- [ ] Clean up hardcoded config values (move to settings)

### Duplicated Logic
- [x] GroundMovementFilter.CanPass duplicated IsTileBlocked
- [x] TrainUnit duplicated TryTrainUnit validations
- [x] FindClosestResource two identical overloads
- [x] DefinitionLoader duplicated (Unit/Building)

---

# Phase 6 — Visibility & Fog of War (planned)

- [ ] Tile visibility system (Hidden / Fog / Visible)
- [ ] Sight range per unit and building
- [ ] Visibility updates per tick
- [ ] Integrate with pathfinding and combat
- [ ] Restrict AI knowledge to visible area

---

# Phase 7 — Lua Scripting (future)

Goal: replace hardcoded AI with data-driven Lua scripts.

- [ ] Embed Lua runtime
- [ ] Create scripting API (read world, issue commands)
- [ ] Migrate AI brains to Lua
- [ ] Event system for Lua callbacks
- [ ] Test AI scripts

---

# Phase 8 — Tooling & Integration (future)

- [ ] Replay system
- [ ] Save/load system
- [ ] Map editor
- [ ] Unity integration layer
- [ ] GUI (non-console renderer)

---
