# IdleRacer — Save Schema

## Implemented: Save V1 (Milestone 0.1D)

Local, versioned JSON save written with Unity `JsonUtility` to
`Application.persistentDataPath/idleracer_save_v1.json` by `LocalJsonSaveRepository`
(infrastructure layer). The domain defines the DTO and the `IGameSaveRepository` interface and
never touches the file system. Writes go to a `.tmp` file then atomically replace the primary
file. `PlayerPrefs` is not used.

### V1 fields (`GameSaveDataV1`)

- `saveVersion` (int) — current version = 1.
- `lastSavedUtcTicks` (long) — UTC ticks at save time (for offline-duration calculation).
- `gold` (long)
- `wheels` (long)
- `campaignStageIndex` (int) — index into the Normal-1 campaign.
- `autoBuildUnlocked` (bool)
- `itemCreatorLevel` (int), `itemCreatorXp` (int)
- `slotLevels` (int[8]) — permanent per-slot levels, indexed by `(int)EquipmentSlotType`.
- `equippedItems` (list of `EquipmentItemDto`: id, slot, rarity, accelerationBonus, topSpeedBonus)
- `hasPendingItem` (bool) + `pendingItem` (`EquipmentItemDto`)

### Not persisted (transient presentation state)

Car visual positions, READY/GO! text, race animation elapsed time, open UI panel/scroll, and
any in-flight race playback are never saved.

### Pending-item behaviour

If a generated item is awaiting EQUIP/DISCARD, it IS persisted. This prevents closing/reopening
the app from being used to reroll an unwanted generated item for free (the Wheel was already
spent and the item is fixed).

### RNG state

The item-generation RNG's internal state is intentionally not persisted. Because generation
results are saved as they happen (and the pending item is persisted), a per-session reseed
cannot reset or exploit progression.

### Versioning & failure behaviour

- Missing save → a fresh player state is created from `GameConfig` defaults (single source).
- Corrupted/unparseable save → reported `Corrupted`, treated as fresh (never crashes).
- `saveVersion` newer than supported → reported `UnsupportedVersion`, treated as fresh.
- Migrations: none needed at V1. When the schema changes, bump `SaveConstants.CurrentVersion`
  and add a migration path (no large migration framework built yet).

### Offline duration

`OfflineProgress.CalculateOfflineDuration(lastSavedUtc, nowUtc, maxDuration?)` uses UTC, clamps
negative durations (clock changes) to zero, and optionally caps to a maximum. v0.1D grants NO
offline rewards — the value is only logged as a foundation for a later milestone.

---

## Future conceptual structure

The following are future systems not yet implemented; do not add empty structures for them yet.

Conceptual structure:

GameSaveData

- SchemaVersion
- PlayerProfile
- CurrencyState
- CampaignState
- EquipmentInventory
- EquippedLoadout
- EquipmentSlotProgression
- ItemCreatorState
- RefinementState
- UnlockState
- OfflineProgressionState
- SkillState
- CompanionState
- TrainerState
- DungeonState
- PrestigeState
- PrestigeTreeState
- CollectionState
- ArenaState

Initial implementation should only include systems that actually exist.

Do not prematurely add empty save structures for every future feature.

Save migrations are mandatory when persisted schemas change.
