# RTSEngine
Tick-based deterministic RTS engine in C# designed for replayability, AI simulation and future Unity integration.

## Design Goals

The simulation core is intentionally renderer-independent.

Rendering, input handling and networking are planned as separate layers in order to preserve deterministic simulation behavior.

## Project Structure

RTSEngine.Core -> deterministic simulation logic

RTSEngine.DebugClient -> debug visualization and runtime testing

RTSEngine.Tests -> unit and integration tests

## RoadMap
