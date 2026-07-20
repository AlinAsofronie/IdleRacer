# IdleRacer — Save Schema

Status:
Design placeholder only.

No save implementation exists yet.

Future saves must contain a schema version.

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
