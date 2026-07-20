# IdleRacer — Game Design Document

## 1. Game Concept

IdleRacer is a mobile-first incremental racing RPG.

The player is not primarily the driver.

The player builds, upgrades and optimises the machine.

The car races automatically while the player interacts with progression systems.

The core fantasy is:

"You are not the driver. You are the mastermind behind the car."

The player should be able to watch their car improve visibly as their build gets stronger.

The game combines:

- idle progression,
- equipment loot,
- auto racing,
- build specialisation,
- dungeons,
- asynchronous PvP,
- long-term Prestige progression,
- collection systems,
- multiple race disciplines.

---

## 2. Primary Screen Philosophy

The main screen is split into two conceptual areas.

### Top Area

Continuous race gameplay.

The player sees:
- their car,
- opponent cars,
- track,
- overtakes,
- acceleration,
- speed differences,
- nitrous effects,
- skills,
- companions,
- race events,
- stage progression.

### Bottom Area

Progression and interaction.

The player manages:
- Item Creator,
- equipment,
- Gold upgrades,
- skills,
- companions,
- trainers,
- dungeons,
- race modes,
- Prestige,
- Arena.

The race should continue in the top area while the player interacts with the bottom area wherever practical.

---

## 3. Core Loop

Auto Race
→ Earn Gold and Wheels
→ Generate Equipment
→ Compare and Equip Items
→ Upgrade Equipment Slots
→ Improve Build
→ Beat Current Stage
→ Progress Automatically
→ Unlock New Systems
→ Reach Progression Wall
→ Use Dungeons / Refinement / Skills / Companions
→ Push Further
→ Prestige
→ Spend Prestige Currency
→ Repeat Faster

---

## 4. Campaign

Campaign progression is fully automated.

The player does not choose each individual race.

Winning moves the player automatically to the next stage.

Losing causes automatic retries of the same stage.

Initial structure:

### Normal
- 1-1 to 1-10
- 2-1 to 2-20
- 3-1 to 3-30

### Hard
- 1-1 to 1-10
- 2-1 to 2-20
- 3-1 to 3-30

### Hell
- 1-1 to 1-10
- 2-1 to 2-20
- 3-1 to 3-30

Additional difficulty tiers may be added later.

---

## 5. Currency

### Gold

Gold is earned from:
- active racing,
- stage progression,
- offline progression,
- selected dungeons,
- events.

Gold is primarily used to upgrade the eight permanent equipment slots.

### Wheels

Wheels are earned from:
- completing races,
- progressing stages,
- bosses,
- offline progression,
- events.

One Wheel normally creates one equipment item.

### Refinement Currency

Earned primarily from a dedicated Refinement Dungeon.

Used to increase individual equipment refinement stars.

### Prestige Currency

Earned through Awakening / Prestige.

Used on the permanent Prestige Tree.

---

## 6. Equipment Slots

There are eight equipment slots:

1. Engine
2. Turbo
3. Gearbox
4. Tyres
5. Suspension
6. ECU
7. Exhaust
8. Fuel System

The slot has a permanent level.

The equipped item is separate.

Replacing an item does not reset the permanent slot level.

---

## 7. Equipment Generation

The player uses an Item Creator.

Each attempt normally costs:
- 1 Wheel.

The player manually generates equipment at the start of the game.

The Item Creator has its own level.

Higher levels improve rarity probabilities.

The UI must clearly display actual rarity chances.

Potential rarity ladder:

- Common
- Uncommon
- Rare
- Epic
- Legendary
- Mythic

Each generated item may contain:

- equipment slot type,
- rarity,
- base stats,
- percentage modifiers,
- affixes,
- tags,
- refinement stars.

---

## 8. Auto Build

Auto Build is a campaign progression reward.

Initial design target:

- Normal 1-10 → Auto Build x1
- Normal 2-20 → Auto Build x2
- Normal 3-30 → Auto Build x3
- Hard 1-10 → Auto Build x4
- Hard 2-20 → Auto Build x5
- Hard 3-30 → Auto Build x6
- Hell 1-10 → Auto Build x7
- Hell 2-20 → Auto Build x8
- Hell 3-30 → Auto Build x10

Each generated item still consumes one Wheel.

Auto Build does not improve rarity.

Item Creator Level controls rarity.

Later convenience systems may include:
- auto dismantle,
- rarity filters,
- stop-on-high-rarity,
- review tray.

---

## 9. Gold Slot Upgrades

Gold permanently upgrades each equipment slot.

Example:

Engine Slot Lv. 37

The level remains when the player changes Engine equipment.

Slot levels provide:
- base stat growth,
- milestone bonuses.

Milestone progression should feel significant.

Future milestone examples:
- Lv. 10
- Lv. 25
- Lv. 50
- Lv. 100
- Lv. 250

Exact bonuses are not final.

---

## 10. Refinement

Refinement belongs to the individual item.

Current target:

- 0 stars → base item stats
- 1 star → +5% all item stats
- 2 stars → +10%
- 3 stars → +15%
- 4 stars → +20%
- 5 stars → +25%

Higher star upgrades cost more Refinement Currency.

A future recovery or transfer mechanism should prevent players from feeling punished for investing in good equipment before finding a better item.

---

## 11. Build Specialisation

Players should not maximise all performance categories simultaneously.

Core build directions include:

### Acceleration
Strong launch and speed recovery.
Weaker maximum top speed.

### Top Speed
Slower launch.
Very strong long straight performance.

### Nitro
Strong burst performance.
Focus on nitrous activation, duration and generation.

### Grip / Circuit
Lower speed loss through corners.

### Skill Build
Stronger or more frequent skill activation.

### Hybrid
Balanced performance.

Build choices should matter across race modes.

---

## 12. Equipment Tags and Synergies

Equipment may contain tags.

Example tags:

- Street
- Turbo
- Acceleration
- TopSpeed
- Drag
- Circuit
- Grip
- Skill

Combining tags may unlock synergies.

The goal is flexible theorycrafting rather than rigid armour-style set bonuses.

---

## 13. Race Disciplines

### Main Campaign

General progression.

### Drag Racing

Separate progression system.

Rewards:
- acceleration,
- launch,
- top speed,
- gear shift performance,
- nitrous.

### Circuit Racing

Separate progression system.

Rewards:
- grip,
- braking,
- cornering,
- acceleration out of corners.

Possible future disciplines:
- Rally
- Endurance

---

## 14. Skills

Skills may activate automatically.

The player may optionally use manual activation where supported.

Examples of future skill concepts:

- Perfect Launch
- Overdrive
- Slipstream
- Redline
- Last Lap

Skills should visually change races.

---

## 15. Companions

Companions may race alongside or support the player's car.

Potential examples:

- Drone
- Motorbike
- Support Vehicle
- Pursuit Car

Companions provide passive or triggered bonuses.

---

## 16. Trainers

Trainers provide passive build bonuses.

Players may eventually equip a limited number.

Trainer choices should contribute to build specialisation.

---

## 17. Dungeons

Dungeons are limited-attempt activities.

Target:
- approximately 2 free attempts per day for relevant dungeons.

Possible dungeons:

### Refinement Dungeon
Rewards Refinement Currency.

### Gold Rush
Rewards Gold.

### Skill Dungeon
Rewards Skill resources.

### Companion Dungeon
Rewards Companion resources.

### Trainer Dungeon
Rewards Trainer resources.

Dungeons should use racing mechanics.

---

## 18. Dynamic Events

Occasional automatic race events prevent visual repetition.

Potential examples:

### Rival Challenger
Unique opponent and bonus reward.

### Parts Truck
Defeat it for Wheels.

### Golden Car
Catch it for bonus Gold.

### Police Chase
Special survival race.

### Sudden Rain
Temporary track conditions that favour grip.

Events require no driving input.

---

## 19. PvP Arena

PvP is asynchronous.

Players race snapshots of other players' builds.

The race is simulated locally or server-coordinated using validated build data.

Arena includes:

- rating,
- ladder,
- ranks,
- seasonal rewards.

Potential ranks:

- Bronze
- Silver
- Gold
- Platinum
- Diamond
- Master
- Grandmaster
- Legend

The player may eventually use specialised Arena loadouts.

Opponent stats may be partially visible before the race without exposing every exact number.

---

## 20. Prestige / Awakening

Prestige unlock target:

Hard 3-30 completion.

After unlocking:
- Prestige remains available permanently.
- The player chooses when to use it.

Prestige grants Prestige Currency based on progression depth.

The player should face an optimisation decision:

Prestige now for immediate permanent power

or

Push further for more Prestige Currency.

The exact reset rules remain undecided and must be finalised before implementation.

---

## 21. Prestige Tree

Prestige Currency is spent on a large interconnected passive tree.

The tree contains:

- small passive nodes,
- major nodes,
- keystones,
- branching paths,
- cross-build routes.

Potential categories:

- Acceleration
- Top Speed
- Nitro
- Grip
- Drag
- Circuit
- Skills
- Equipment
- Economy

Keystones should alter builds and include trade-offs.

Examples:

Large acceleration increase
in exchange for lower maximum speed.

Large top-speed increase
in exchange for slower acceleration.

---

## 22. Temporary Prestige Traits

A future system may allow the player to choose temporary traits during each Prestige run.

These reset on the next Prestige.

Purpose:
- make individual runs feel different,
- introduce temporary build experimentation.

---

## 23. Stage Skipping

Prestige should make early content dramatically faster.

Future ideas:

- Domination Victory
- Skip 1 stage
- Skip 3 stages
- Skip 5 stages
- larger skips based on victory margin and account progression.

The player should feel visibly stronger after Prestige.

---

## 24. Offline Progression

The car continues generating progress while the player is away.

Offline rewards may include:

- Gold
- Wheels

Returning players receive a summary.

Example:

Welcome Back

Time Away: 8h 14m

Gold: +42,850
Wheels: +287

Offline caps should be configurable.

---

## 25. Collections

Collection categories may include:

- cars,
- equipment,
- companions,
- skills,
- tracks,
- bosses.

Completing collections may give small permanent bonuses.

---

## 26. Reward Cadence

The game should operate across multiple reward timescales.

### Every few seconds
- race activity,
- Gold gain,
- Wheel progress,
- item generation.

### Every few minutes
- equipment upgrade,
- stage boss,
- slot milestone,
- rare drop.

### Every 15–30 minutes
- system unlock,
- dungeon activity,
- major campaign milestone.

### Daily
- dungeon attempts,
- Arena progress,
- refinement progression.

### Long-Term
- Prestige,
- Prestige Tree,
- Mythic gear,
- build theorycrafting,
- seasonal PvP,
- collection completion.

---

## 27. Core Design Rule

The player should always feel:

"There is something happening, something to upgrade, and something just within reach."
