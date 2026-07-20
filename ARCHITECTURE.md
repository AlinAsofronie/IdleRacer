# IdleRacer — Technical Architecture

## 1. Architecture Goal

IdleRacer must remain modular and data-driven.

The most important architectural rule is:

Race Simulation and Race Visualisation are separate systems.

The simulation determines what happens.

The visual layer displays what happened.

---

## 2. High-Level Layers

### Domain Layer

Pure C# wherever practical.

Contains:
- race simulation,
- stats,
- modifiers,
- equipment calculations,
- currency rules,
- progression rules,
- Prestige calculations.

Should not depend on:
- scene objects,
- cameras,
- particles,
- UI.

### Application Layer

Coordinates use cases.

Examples:
- GenerateItemCommand
- UpgradeSlotCommand
- StartRaceCommand
- PrestigePreviewService

### Presentation Layer

Unity-specific.

Contains:
- MonoBehaviours,
- UI,
- animation,
- scene integration,
- effects.

### Infrastructure Layer

Contains:
- local save implementation,
- future cloud save,
- backend service adapters,
- analytics,
- Unity Gaming Services integration.

---

## 3. Race Architecture

Conceptual input:

RaceSimulationRequest

Containing:
- PlayerRaceStats
- OpponentRaceStats
- TrackDefinition
- SkillLoadout
- RaceModifiers
- RandomSeed

Conceptual output:

RaceSimulationResult

Containing:
- Winner
- FinishTimes
- RaceTimeline
- KeyRaceEvents
- FinalDistances
- VictoryMargin

Race visuals consume RaceSimulationResult.

---

## 4. Shared Simulation

The same simulator should eventually support:

- Campaign
- Drag
- Circuit
- Dungeons
- Dynamic Events
- Arena

Race modes configure:
- track,
- opponent,
- modifiers,
- reward rules.

They should not each contain completely separate race engines.

---

## 5. Stats

Future central stat system may include:

- Acceleration
- TopSpeed
- Grip
- Braking
- Cornering
- GearShiftSpeed
- NitroPower
- NitroDuration
- NitroGeneration
- SkillCooldown

Do not implement all immediately.

Initial race prototype should begin with:

- Acceleration
- TopSpeed

---

## 6. Modifier Pipeline

Planned future order:

Base Stats

→ Flat Additions

→ Additive Percentage Bonuses

→ Multiplicative Bonuses

→ Conditional Modifiers

Sources:

- equipment,
- slot levels,
- refinement,
- affixes,
- tags,
- skills,
- companions,
- trainers,
- Prestige Tree,
- track effects.

The exact formula must be formally documented before implementation.

---

## 7. Configuration

Static balancing data should eventually live in configuration assets or equivalent data definitions.

Examples:

- StageDefinition
- DifficultyDefinition
- EquipmentRarityDefinition
- ItemCreatorLevelDefinition
- AutoBuildUnlockDefinition
- SlotUpgradeDefinition
- RefinementDefinition
- TrackDefinition
- DungeonDefinition
- PrestigeDefinition

Do not hard-code progression tables into UI classes.

---

## 8. Save Architecture

Save data should be versioned.

Conceptual separation:

GameSaveData
- version
- currencies
- campaign
- inventory
- equipment
- slot upgrades
- Item Creator
- refinement
- unlocks
- future progression systems

Save repository interface:

IGameSaveRepository

Initial implementation:
LocalGameSaveRepository

Future implementation:
CloudGameSaveRepository

Game logic should not care which persistence implementation is used.

---

## 9. Economy Architecture

All currency mutations should eventually flow through:

IEconomyService

Conceptual operation:

TrySpend(currency, amount, reason)

Grant(currency, amount, reason)

Reason must be tracked.

UI must never set balances directly.

---

## 10. Equipment Architecture

Conceptual models:

EquipmentItem
EquipmentSlotType
EquipmentRarity
EquipmentAffix
EquipmentTag
EquipmentRefinement

Player equipment state:

EquipmentInventory
EquippedLoadout
EquipmentSlotProgression

Generated item identity must remain separate from slot progression.

---

## 11. Item Generation

Conceptual flow:

Wheel spent

→ ItemGenerator receives ItemCreatorLevel

→ Select rarity from configured probabilities

→ Select compatible equipment slot

→ Generate base stats

→ Roll affixes

→ Roll tags

→ Create unique EquipmentItem

→ Add to inventory

Random generation should support deterministic seeded testing.

---

## 12. Prestige Architecture

Prestige should be divided into:

PrestigeEligibilityService
PrestigeRewardCalculator
PrestigePreview
PrestigeExecutionService
PrestigeTree

Do not embed Prestige reset logic in UI.

---

## 13. PvP Architecture

Future asynchronous PvP flow:

Player requests opponents

→ backend returns opponent snapshots

→ player selects opponent

→ race uses validated snapshot

→ result submitted for server validation

→ rating and rewards updated

PvP snapshot should be serialisable and versioned.

---

## 14. Folder Structure

Use this general project structure:

Assets/
  Game/
    Core/
      Economy/
      Progression/
      Statistics/
      SaveSystem/
      Events/

    Racing/
      Simulation/
      Visuals/
      Tracks/
      Opponents/

    Equipment/
      Items/
      Generator/
      Rarities/
      Affixes/
      Tags/
      Refinement/

    Progression/
      Campaign/
      Prestige/
      SlotUpgrades/
      Unlocks/

    Skills/
    Companions/
    Trainers/
    Dungeons/
    Arena/

    UI/

    Services/
      Authentication/
      CloudSave/
      Leaderboards/
      CloudCode/

  Art/
  Audio/
  Prefabs/
  Scenes/
  Tests/

This structure may evolve.

Do not create architecture merely for empty abstraction.

---

## 15. Testing Strategy

Use automated tests for pure domain logic.

Priority tests:

1. Race simulation
2. Deterministic race outcomes
3. Item rarity probabilities
4. Equipment stat calculations
5. Modifier stacking
6. Refinement
7. Currency spending
8. Slot upgrades
9. Offline progression
10. Prestige calculations
11. Save migrations

---

## 16. Mobile Performance

The game should not depend on continuous realistic physics simulation.

The simulation should be mathematical.

The visual layer may interpolate movement based on the simulation.

Target:
- smooth mobile performance,
- low battery impact,
- low thermal load,
- scalable visual effects.

---

## Race Simulation v0.1

The first implemented slice of the authoritative race simulator. It lives in
`Assets/Game/Racing/Simulation/` as a dedicated pure-C# assembly
(`IdleRacer.Racing.Simulation`, `noEngineReferences: true`) and does not reference
UnityEngine.

### Units

- distance: metres (m)
- speed: metres per second (m/s)
- acceleration: metres per second squared (m/s^2)
- time: seconds (s)

Internally the simulator uses `double` for precision and deterministic results.

### Supported stats (v0.1)

Each car (`CarRaceStats`) currently has only:

- Acceleration (m/s^2)
- TopSpeed (m/s)

A race (`RaceSimulationRequest`) currently has only:

- PlayerStats, OpponentStats
- TrackDistance (m)
- FixedTimeStep (s)

The outcome (`RaceSimulationResult`) reports Winner (`RaceWinner`: Player / Opponent /
Draw), PlayerFinishTime, OpponentFinishTime, and VictoryMarginSeconds.

### Deterministic fixed-timestep approach

Each car is simulated independently. Both start from rest at distance 0 and accelerate at
their constant Acceleration, clamped so speed never exceeds TopSpeed, until they reach
TrackDistance. The simulation advances in fixed `FixedTimeStep` steps. There is no
randomness, so identical inputs always produce identical results. A large iteration guard
protects against accidental infinite loops from invalid input.

### Finish-line interpolation

Finish times are not rounded up to the end of a timestep. Within each step the motion is
integrated in closed form: a constant-acceleration phase followed (if TopSpeed is reached
mid-step) by a constant-speed cruise phase. When the finish line falls inside a step, the
exact crossing instant is solved directly — the quadratic `0.5*a*tau^2 + v*tau - remaining = 0`
during acceleration, or `remaining / TopSpeed` while cruising. This makes finish times
effectively independent of the chosen FixedTimeStep.

### Separation from Unity visuals

The simulator is authoritative and self-contained. It has no dependency on GameObject,
Transform, Rigidbody, physics, animation, cameras, particle systems, or UI. Future race
visuals will only display the `RaceSimulationResult`; visuals never determine the winner.

### Known limitations (v0.1)

- straight tracks only;
- no corners;
- no nitrous;
- no skills;
- no gear shifting;
- no random events;
- no equipment modifiers;
- starts from rest only.
