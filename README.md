# IdleRacer

IdleRacer is a mobile-first incremental racing RPG built with Unity 6 and C#.

The player does not directly drive the car during the core gameplay loop.

Cars race automatically while the player manages equipment, upgrades, item generation, builds and long-term progression.

## Core Concept

Watch the race.
Earn resources.
Generate equipment.
Improve the car.
Progress further.
Build specialised racing strategies.
Prestige.
Repeat faster and deeper.

## Primary Platform

iOS

Future targets may include:
- Android
- Web

## Technology

- Unity 6
- C#
- Universal Render Pipeline
- Git / GitHub

Future online systems may use Unity Gaming Services.

## Current Status

Pre-production / architecture setup.

No production gameplay systems have been implemented yet.

## Documentation

- GAME_DESIGN.md
- ARCHITECTURE.md
- ECONOMY.md
- ROADMAP.md
- SAVE_SCHEMA.md
- CHANGELOG.md

## Development Philosophy

The race simulation is independent from the race visuals.

Core gameplay systems should be:

- modular,
- deterministic where practical,
- data-driven,
- testable,
- mobile-friendly.

The initial goal is a small vertical slice proving the core gameplay loop before building advanced systems.
