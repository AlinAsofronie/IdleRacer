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

---

## Race Visualisation v0.1B

The first presentation-layer prototype that proves the authoritative simulator can drive a
visually satisfying race. It lives in `Assets/Game/Racing/Visuals/`
(`IdleRacer.Racing.Visuals`, which references `IdleRacer.Racing.Simulation` and
`UnityEngine.UI`).

### Authoritative simulator vs presentation layer

- The simulator (`IdleRacer.Racing.Simulation`) remains authoritative. It has no Unity
  dependency and decides the winner and finish times.
- The presentation layer (`RacePrototypeController`, `RaceCarView`) only *displays* a race.
  It calls `RaceSimulator.Simulate` for a `RaceSimulationResult` and shows that result's
  winner and finish times. Visuals never determine the outcome.
- No authoritative race calculations live in MonoBehaviours.

### How visual position is derived

Car positions come from the shared pure-C# helper
`RaceKinematics.DistanceAtTime(stats, elapsed)` (in the simulation assembly), so the same
kinematic model (accelerate from rest, cap at TopSpeed) is used by both the simulator and
the visuals — no duplicated formula in the visual layer. Each frame the controller converts
distance-at-time into a normalised progress `distance / TrackDistance` (clamped to [0,1]),
and `RaceCarView` linearly maps that to a world X between the visual start and finish lines.
Playback runs in real time until the later of the two finish times; the faster car clamps at
the finish line while the slower car keeps moving until it finishes.

### Current prototype scene

`Assets/Scenes/RacePrototype.unity` (SampleScene is untouched). It contains a Main Camera,
a Directional Light, and a `RacePrototype` object with `RacePrototypeController`, which
builds the rest at runtime: an orthographic camera framed for portrait (fits a fixed track
width to any aspect), two placeholder cube cars (player = blue, opponent = red), a road, a
start line, a bright finish line, and a Screen Space Overlay UI (READY / GO! / winner
status, both finish times, and a static bottom placeholder panel labelled
"Progression UI Coming Next" occupying the lower ~55%). The race auto-runs, shows the
result, and restarts in a continuous loop with no user input.

### Current limitations (v0.1B)

- placeholder primitive visuals only (no art);
- runtime-built scene contents (only camera/light/controller are authored in the scene);
- fixed camera, straight track, two cars;
- prototype-only race values (not game balance);
- no input, economy, progression, equipment, skills, or audio;
- bottom panel is a static placeholder (no real progression UI yet).

---

## Incremental Core Loop v0.1C

The first playable loop. All game logic is pure C# in the `IdleRacer.Game.Domain` assembly
(`noEngineReferences`), composed by `GameController`. The presentation layer
(`IdleRacer.Racing.Visuals`) only orchestrates timing, race playback, and UI wiring, and
holds no game logic or currency mutation.

### Campaign flow

Data-driven via `StageDefinition` / `CampaignDefinition` (v0.1C ships Normal 1-1 .. 1-10 in
`GameConfig`). `CampaignService` owns the rules: `GameController.PrepareRace()` builds a
`RaceSimulationRequest` from the player's calculated stats and the current stage's opponent,
runs the authoritative `RaceSimulator`, and returns a `RacePlan`. After visual playback,
`GameController.ResolveRace(plan)` calls `CampaignService.ApplyRaceResult(playerWon)`: a win
grants the stage's Gold/Wheels and advances (staying on 1-10 to keep it repeatable and
unlocking Auto Build); a loss/draw grants nothing and retries the same stage.

### Economy flow

`IEconomyService` / `EconomyService` is the single place currencies change; every change
records a `TransactionReason`. UI never mutates balances. Gold accumulates (no sinks yet);
Wheels are spent by the Item Creator (1 per item).

### Item generation

`ItemGenerator` samples a data-driven `RarityTable` and `RarityStatTable` with a supplied
`System.Random` (deterministic per seed) to produce an immutable `EquipmentItem` (id, slot,
rarity, flat Accel/Top bonuses). Displayed odds are the exact table used to generate.

### Equipment stat calculation

`PlayerStatsCalculator`: Final = Base player stats + sum of equipped item flat bonuses →
`CarRaceStats`. `EquipmentLoadout` holds one item per slot; equipping only ever affects that
slot. Equipment changes are reflected in the next race automatically.

### Item Creator progression

`ItemCreator` tracks Level and XP against `ItemCreatorConfig` (prototype: 3 levels). Each
generated item grants 1 XP; reaching the level threshold activates a different (better)
rarity table. All values are configuration-driven.

### Auto Build unlock

Locked until Normal 1-10 is beaten (`CampaignService`). Once unlocked, `GameController`'s
Auto Build runs the exact same generation path/odds as manual build, costs 1 Wheel per item,
stops at zero Wheels, and pauses while an unresolved pending item awaits EQUIP/DISCARD (so
items are never silently destroyed).

### Serialisable-friendly state

Player state (economy balances, campaign index + Auto Build flag, loadout, creator level/XP)
lives in the domain services, not scattered across MonoBehaviours; `GameController` accepts an
optional starting `CampaignState`, anticipating save/load in a later milestone.

### Known limitations (v0.1C)

- Normal 1 only (no Normal 2/3, Hard, Hell); no Gold sinks/slot upgrades;
- no inventory (pending item is single-slot; DISCARD removes permanently);
- no persistence yet; progression UI is code-built (prefab migration deferred);
- prototype rarity/stat/stage values are not final balance.

---

## Permanent Slot Progression v0.1D

Each of the eight equipment slots has a permanent level owned by the SLOT, never by the item
(`EquipmentSlotProgression`; slot level is never stored on `EquipmentItem`). Gold funds
upgrades via `SlotUpgradeService` (spends through `IEconomyService` with reason
`EquipmentSlotUpgrade`; +1 level on success, nothing changes on insufficient Gold). The curve
is one shared data-driven `SlotUpgradeConfig` (prototype: +0.5 Accel / +1.0 Top per level;
cost 50 then doubling — 50, 100, 200, 400 …). All values are PROTOTYPE, not final.

Final player stats now flow through the single authoritative `PlayerStatsCalculator`:

Base Player Stats + Σ(slot-level bonuses) + Σ(equipped item bonuses) = Final Race Stats

The next race after an upgrade automatically uses the improved stats (no duplicated math in UI).

## Local Save System v1

Versioned local persistence with a clean layering boundary:

- Domain (`Assets/Game/Core/SaveSystem/`, pure C#): the `GameSaveDataV1` DTO (+ `EquipmentItemDto`),
  `IGameSaveRepository`, `SaveLoadResult`/`SaveLoadStatus`, `SaveConstants.CurrentVersion`,
  `InMemorySaveRepository` (tests), and `OfflineProgress`. `GameController` maps state ↔ DTO
  (`CreateSaveData` / load-from-DTO constructor) and never touches the file system.
- Infrastructure (`Assets/Game/Infrastructure/`, Unity): `LocalJsonSaveRepository` writes a small
  versioned JSON file under `Application.persistentDataPath` using Unity `JsonUtility`. Writes go
  to a `.tmp` file then atomically replace the primary file, so a partial write is unlikely to
  destroy progression. No `PlayerPrefs` is used for the save.

Save triggering is centralised: `GameController` raises `StateChanged` after any persistent
change (race reward/advance, item build, equip, discard, slot upgrade, Auto Build unlock) and the
`RacePrototypeController` persists on that event plus on application pause/focus-loss/quit — never
per frame.

Load behaviour: missing save → fresh player (from `GameConfig` defaults, single source);
corrupted save → reported `Corrupted`, treated as fresh (no crash); newer `saveVersion` than
supported → reported `UnsupportedVersion`, treated as fresh. The pending generated item is
persisted so restarting the app cannot be used to reroll it for free.

`LastSavedUtc` (UTC ticks) is stored; on load, `OfflineProgress.CalculateOfflineDuration`
computes the away time (clamped to ≥ 0, optional cap). v0.1D grants NO offline rewards yet — the
duration is only logged, as a foundation for a later milestone.
