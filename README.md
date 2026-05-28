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
## Phase 1 — Core Simulation

Goal:
Build a deterministic, renderer-independent RTS simulation core.

---

## World Foundation

- [x] Solution architecture setup
- [x] Engine/client/test project separation
- [x] Initial tile map runtime
- [x] Tile-based grid world
- [x] Grid position abstraction
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
- [ ] World state management

---

## Simulation Core

- [x] Basic simulation loop
- [x] Simulation runner
- [x] Runtime world container
- [ ] Fixed deterministic tick pipeline
- [x] Basic entity runtime
- [ ] Entity movement system
- [ ] Command queue system
- [ ] State management layer

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
- [ ] Resource placement validation
- [ ] Spawn validation rules
- [ ] Procedural map generation

---

## Rendering

- [x] Console renderer
- [x] Colored terrain rendering
- [x] Colored resource rendering
- [x] Colored spawn rendering
- [x] Minimal render mode
- [x] Extended render mode
- [x] UTF-8 symbol rendering
- [ ] Runtime debug controls
- [ ] Tick pause system
- [ ] Debug overlays

---

## Movement

- [ ] 8-direction tile movement
- [ ] Tile occupancy validation
- [ ] Collision system
- [ ] Basic pathfinding foundations

---

## Testing

- [x] Unit testing infrastructure
- [x] Tile map validation tests
- [x] JSON map loading tests
- [x] Tile type mapping tests
- [x] Resource definition tests
- [x] Builder validation tests
- [ ] Tick determinism tests
- [ ] Movement system tests
- [ ] Resource runtime tests

---

# Phase 2 — Gameplay Systems

## Economy

- [ ] Resource economy
- [ ] Resource gathering
- [ ] Resource depletion
- [ ] Deposit system
- [ ] Carry capacity system

---

## Buildings

- [ ] Building placement
- [ ] Construction system
- [ ] Multi-tile structures
- [ ] Production buildings

---

## Combat

- [ ] Combat system
- [ ] Health and damage
- [ ] Attack cooldown system
- [ ] Target selection system

---

## Vision

- [ ] Fog of war
- [ ] Vision memory
- [ ] Visibility updates

---

# Phase 3 — AI Systems

- [ ] AI command system
- [ ] Build order execution
- [ ] Reactive AI behaviors
- [ ] Scout system
- [ ] Combat decision making
- [ ] Economy management AI

---

# Phase 4 — Tooling & Integration

- [ ] Replay system
- [ ] Save/load system
- [ ] Map editor
- [x] Colored debug visualization
- [ ] Runtime debug controls
- [ ] Debug visualization improvements
- [ ] Unity integration layer
