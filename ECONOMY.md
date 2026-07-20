# IdleRacer — Economy Design

## Core Currencies

### Gold

Sources:
- race completion,
- campaign progression,
- offline rewards,
- events,
- Gold dungeon.

Primary sink:
- equipment slot upgrades.

Design goal:
reliable guaranteed progression.

---

### Wheels

Sources:
- race completion,
- campaign progression,
- bosses,
- dynamic events,
- offline rewards.

Primary sink:
- Item Creator.

Default item-generation cost:
- 1 Wheel.

Design goal:
loot-generation frequency and excitement.

---

### Refinement Currency

Primary source:
- Refinement Dungeon.

Primary sink:
- increasing stars on individual equipment items.

Design goal:
long-term investment into high-quality gear.

---

### Prestige Currency

Primary source:
- Awakening / Prestige.

Primary sink:
- Prestige Tree.

Design goal:
permanent meta progression.

---

## Economy Principles

1. Gold provides predictable progress.
2. Wheels provide random item opportunities.
3. Refinement Currency rewards commitment to strong gear.
4. Prestige Currency provides permanent account progression.
5. Do not allow one currency to replace every system.
6. Do not create unnecessary currency bloat.
7. Every currency must have a clear purpose.
8. Item rarity probabilities must be transparent.
9. Do not secretly modify advertised drop chances.
10. Competitive rewards must eventually be server validated.

---

## Item Creator

Cost:
1 Wheel per item by default.

Item Creator Level:
improves rarity odds.

Auto Build:
improves throughput, not rarity.

---

## Offline Economy

Offline rewards may include:

Gold
Wheels

Offline cap:
not yet final.

Must be configuration-driven.

---

## Prototype Values (Milestone 0.1C)

These are TEMPORARY prototype values, not final balance.

Starting balances:
- Gold: 0
- Wheels: 5

Gold sources (v0.1C):
- winning a campaign stage (per-stage `GoldReward`, 50 → 250 across Normal 1-1..1-10).

Gold sinks (v0.1C):
- none yet (Gold accumulates; slot upgrades come later).

Wheel sources (v0.1C):
- winning a campaign stage (per-stage `WheelReward`, 2 → 6).

Wheel sinks (v0.1C):
- Item Creator: 1 Wheel per generated item (manual and Auto Build).

Transaction reasons recorded: `InitialGrant`, `RaceReward`, `ItemGenerationCost`.

Item Creator rarity odds (must sum to 100%, displayed exactly as used):
- Level 1: Common 60, Uncommon 25, Rare 10, Epic 4, Legendary 0.9, Mythic 0.1
- Level 2: Common 45, Uncommon 30, Rare 15, Epic 7, Legendary 2.5, Mythic 0.5
- Level 3: Common 30, Uncommon 30, Rare 22, Epic 12, Legendary 5, Mythic 1

All of the above are prototype-only and subject to change.

## Prototype Values (Milestone 0.1D)

Gold now has its first sink: permanent equipment slot upgrades.

Gold sinks (v0.1D):
- equipment slot upgrades (via `SlotUpgradeService`, reason `EquipmentSlotUpgrade`).

Prototype slot-upgrade cost curve (shared across all eight slots): cost to go from level L is
`50 * 2^(L-1)` → Level 1→2: 50, 2→3: 100, 3→4: 200, 4→5: 400, 5→6: 800, …

Permanent slot progression bonus per level: +0.5 Acceleration (m/s^2), +1.0 TopSpeed (m/s).
Slots start at Level 1 (0 bonus). These are PROTOTYPE balance values, not final.

The early upgrades (50, 100) are affordable within the first Normal-1 stage wins (which grant
50–250 Gold), giving Gold a guaranteed-progression role alongside Wheels (random loot).

---

## Monetisation

Monetisation has not yet been designed.

Do not implement:
- premium currency,
- ads,
- in-app purchases,
- pay-to-win systems

without explicit approval and a separate design process.
