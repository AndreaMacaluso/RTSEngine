# RTSEngine
Tick-based deterministic RTS engine in C# designed for replayability, AI simulation and future Unity integration.

## Design Goals

The simulation core is intentionally renderer-independent.

Rendering, input handling and networking are planned as separate layers in order to preserve deterministic simulation behavior.

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

### Phase 1 — Core Simulation

Goal:
Build a deterministic, renderer-independent simulation core.
#### World Foundation
- [x] Solution architecture setup
- [x] Initial tile map structure
- [x] Tile-based grid world
- [x] JSON-based map definitions
- [x] Runtime tile map builder
- [x] Data-driven map loading pipeline
- [x] Runtime asset loading pipeline
- [ ] Terrain properties system
- [ ] World state management

#### Simulation Core
- [x] Basic simulation loop
- [ ] Fixed deterministic tick pipeline
- [ ] Entity system
- [ ] Command queue system
- [ ] State management layer

#### Map System
- [x] Tile type mapping
- [x] Symbol-based terrain parsing
- [x] Tile map validation
- [ ] Walkable tile system
- [ ] Buildable tile system
- [ ] Resource placement system
- [ ] Spawn system

#### Movement
- [ ] 8-direction tile movement
- [ ] Tile occupancy system
- [ ] Collision validation
- [ ] Basic pathfinding foundations

#### Testing
- [x] Unit testing infrastructure
- [x] Tile map validation tests
- [x] Map loading tests
- [x] Tile type mapping tests
- [ ] Tick determinism tests
- [ ] Movement system tests

### Phase 2 — Gameplay Systems

#### Economy
- [ ] Resource economy
- [ ] Resource gathering
- [ ] Resource depletion
- [ ] Deposit system

#### Buildings
- [ ] Building placement
- [ ] Construction system
- [ ] Multi-tile structures

#### Combat
- [ ] Combat system
- [ ] Health and damage
- [ ] Attack cooldown system

#### Vision
- [ ] Fog of war
- [ ] Vision memory

### Phase 3 — AI Systems

- [ ] AI command system
- [ ] Build order execution
- [ ] Reactive AI behaviors
- [ ] Scout system
- [ ] Combat decision making

### Phase 4 — Tooling & Integration

- [ ] Replay system
- [ ] Map editor
- [ ] Debug visualization improvements
- [ ] Unity integration layer
