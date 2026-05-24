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
- [ ] Terrain properties system
- [ ] World state management

#### Simulation Core
- [ ] Fixed tick-based simulation loop
- [ ] Deterministic update pipeline
- [ ] Entity system
- [ ] Command queue system
- [ ] State management layer

#### Movement
- [ ] 8-direction tile movement
- [ ] Tile occupancy system
- [ ] Collision validation
- [ ] Basic pathfinding foundations

#### Testing
- [ ] Unit testing infrastructure
- [ ] Tile map validation tests
- [ ] Tick determinism tests
- [ ] Movement system tests

### Phase 2 — Gameplay Systems

- [ ] Resource economy
- [ ] Building placement
- [ ] Construction system
- [ ] Combat system
- [ ] Health and damage
- [ ] Fog of war
- [ ] Vision memory