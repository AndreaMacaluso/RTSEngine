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
- [ ] Fixed deterministic tick pipeline
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
- [x] Runtime unit rendering
- [ ] Runtime debug controls
- [x] Tick pause system
- [ ] Debug overlays

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
- [ ] Dynamic collision handling

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

---

# Phase 2 — Gameplay Systems

## Economy

- [x] Resource economy
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
- [x] Runtime debug controls
- [ ] Debug visualization improvements
- [ ] Unity integration layer