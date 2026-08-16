# RTSEngine
Tick-based deterministic RTS engine in C# designed for replayability, AI simulation and future Unity integration.

## Design Goals

The simulation core is intentionally renderer-independent.

Rendering, input handling and networking are planned as separate layers in order to preserve deterministic simulation behavior.

## Disclaimer

This project is a personal learning and engineering challenge aimed at building a Real-Time Strategy (RTS) engine from scratch.

Features such as pathfinding, unit management, building construction, resource economy, world state management, and AI are implemented as part of the learning process.

As a result, some solutions may prioritize educational value and architectural clarity over production-level optimization.

## Project Structure

RTSEngine.Core -> deterministic simulation logic

RTSEngine.DebugClient -> debug visualization and runtime testing

RTSEngine.Tests -> unit and integration tests

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
- [ ] End-to-end villager gather cycle
- [ ] Gather interruption handling
- [x] Dynamic deposit selection

### Production Loop

+ [x] Building production queue
+ [x] Unit training command
+ [x] Training progress system
+ [x] Unit spawn from building
+ [x] End-to-end production cycle
+ [x] Resource payment validation
+ [x] Resource payment on production command
+ [ ] Production cancellation / refund

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
- [ ] Cancel construction

## Runtime Gameplay Loop

- [x] Initial economy
- [x] Initial town center
- [x] Villager spawn
- [x] Gather loop
- [x] Construction loop
- [x] Production loop
- [x] Combat loop

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
- [ ] Tick determinism tests
- [ ] Resource runtime tests
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
- [ ] Builder selector tests
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

## Documentation

- [x] Architecture documentation (`Docs/Architecture.md`)
- [x] Loop architecture docs (`Docs/Loops/`)
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
- [ ] Gather interruption
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

- [ ] Multi-tile structures
- [ ] Building cancellation
- [ ] Building refund
- [ ] Repair system
- [ ] Building destruction
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