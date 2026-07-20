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
- First playable incremental core loop v0.1C (`IdleRacer.Game.Domain`): data-driven Normal
  1-1..1-10 campaign, central Economy (Gold/Wheels) with transaction reasons, eight
  equipment slots, seeded item generation with data-driven rarity tables, Item Creator
  level/XP progression, equipment-driven player stats, and Auto Build unlock after Normal
  1-10 (pauses on unresolved items). Progression UI added to the RacePrototype scene.
- Edit Mode tests for the incremental loop (campaign, economy, item generation, equipment,
  Item Creator, Auto Build).
- Permanent Gold-funded equipment slot upgrades v0.1D: per-slot levels (`EquipmentSlotProgression`),
  data-driven `SlotUpgradeConfig` curve, `SlotUpgradeService`, and slot bonuses folded into the
  single authoritative player-stat calculation; slot-upgrade UI added to each equipment slot.
- Local versioned save system V1: `GameSaveDataV1` DTO + `IGameSaveRepository` (domain) and
  `LocalJsonSaveRepository` (infrastructure, atomic JSON writes under persistentDataPath), with
  centralised save triggers, corruption/unsupported-version safe handling, and pending-item
  persistence.
- Offline-duration foundation (`OfflineProgress`, `LastSavedUtc`); no offline rewards yet.
- Edit Mode tests for slot upgrades and the save system.
- Mobile UI shell v0.2A: tabbed portrait progression shell (`GameHudView` + focused panel views)
  with Garage / Build / Upgrades / Race / More navigation, persistent Gold/Wheels resource bar,
  improved pending-item delta card, centralised rarity colours, and safe-area-aware layout.
  Selected tab is presentation-only (defaults to Build; not saved). Replaces the monolithic
  `ProgressionUiView`.
- Edit Mode tests for `HudTabController` tab selection behaviour.
- Playtest-ready mobile UI/UX overhaul v0.2B: TextMeshPro HUD text, central `UiTheme`,
  `CompactNumberFormatter`, redesigned resource bar / Build / Garage / Upgrades / Race / More /
  bottom nav, pending-item reveal animation, and `RaceWorldView` lane/marker/speed-line presentation
  (simulation unchanged). TMP Essential Resources added under `Assets/TextMesh Pro/`.
- Edit Mode tests for compact number formatting.
