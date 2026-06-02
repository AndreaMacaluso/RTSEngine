# RTS Engine Architecture

## Overview

RTS Engine is a learning-oriented Real-Time Strategy engine developed from scratch.

The project is organized around a simulation-driven architecture where game state, systems, and entities are separated from rendering and user interaction.

The current implementation focuses on the foundational RTS systems:

* Tile-based world representation
* Command processing
* Unit movement
* Pathfinding
* Resource nodes
* Simulation loop

---

## Solution Structure

```text
RTSEngine.Core
├── Simulation
├── State
├── Systems
├── Commands
├── Entities
├── Players
└── Map

RTSEngine.DebugClient
└── Console Renderer

RTSEngine.Tests
└── Automated Tests
```

---

## High-Level Architecture

```text
Player Input
      ↓
Command
      ↓
Command System
      ↓
Gameplay Systems
      ↓
Game World
      ↓
Renderer
```

---

## Core Components

### Game World

The GameWorld acts as the central runtime state container.

Responsibilities:

* Store entities
* Store map information
* Store world objects
* Provide access to simulation systems

---

### Simulation Runner

The SimulationRunner is responsible for advancing the simulation.

Responsibilities:

* Execute simulation ticks
* Update systems
* Maintain deterministic game state progression

---

### Commands

Commands represent player intentions.

Current examples:

* MoveCommand

Commands are processed by the CommandSystem and translated into gameplay actions.

---

### Systems

Systems contain game logic.

Current systems:

* CommandSystem
* MovementSystem
* PathSystem
* PathSystemBFS

Systems operate on world entities and modify game state.

---

### Entities

Entities represent objects existing in the game world.

Current categories:

* Units
* Resource Nodes
* Buildings (planned)
* World Objects

Examples:

* Villager
* Tree
* GoldMine
* StoneMine
* BerryBush

---

### Map System

The map subsystem is responsible for loading and constructing game worlds.

Components:

* JsonMapLoader
* TileMapBuilder
* WorldBuilder
* ResourceFactory

Map definitions are stored as JSON files.

---

## Data Flow

```text
JSON Map Data
      ↓
Map Loader
      ↓
World Builder
      ↓
GameWorld
      ↓
Simulation
      ↓
Renderer
```

---

## Current Design Principles

### Simulation First

The engine is developed around simulation logic before graphical presentation.

### Data Driven Content

Maps are defined using external JSON files.

Future content such as units, buildings, technologies, and resources will follow the same approach.

### Separation of Concerns

Game logic, rendering, and testing are maintained in separate projects.

---

## Planned Systems

Future systems include:

* Resource Economy
* Building Construction
* Unit Production
* Combat
* AI
* Technology Tree
* World State Management
* Save / Load Support

```
```
