# Changelog

All notable project changes will be documented in this file.

The project follows a lightweight chronological changelog during early development.

## Unreleased

### Added

- Initial Unity project.
- Git and GitHub repository.
- Project architecture documentation.
- Game Design Document.
- Economy design.
- Development roadmap.
- Save schema planning.
- Cursor project rules.
- MCP tooling: Unity MCP (MCP for Unity) and Context7 integration.
- Headless race simulation v0.1: deterministic, pure-C# two-car straight-track simulator
  (Acceleration and TopSpeed) with fixed-timestep stepping and analytic finish-line
  interpolation, in `IdleRacer.Racing.Simulation`.
- Edit Mode tests for the race simulation (`IdleRacer.Racing.Tests.EditMode`).
- Race visualisation prototype v0.1B: a portrait `RacePrototype` scene and presentation
  layer (`IdleRacer.Racing.Visuals`: `RacePrototypeController`, `RaceCarView`) that plays
  back the authoritative simulation with placeholder cars, a READY/GO!/winner flow,
  finish-time display, a static bottom progression placeholder, and continuous autoplay.
- Shared pure-C# `RaceKinematics.DistanceAtTime` helper (with Edit Mode tests) so the
  visual layer derives car positions from the same kinematic model as the simulator.
