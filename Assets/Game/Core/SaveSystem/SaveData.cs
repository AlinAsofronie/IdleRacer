using System;
using System.Collections.Generic;

namespace IdleRacer.Game.Core.SaveSystem
{
    /// <summary>
    /// Current save schema version. Increment when the persisted structure changes and add a
    /// migration path.
    /// </summary>
    public static class SaveConstants
    {
        public const int CurrentVersion = 1;
    }

    /// <summary>
    /// Serialisable DTO for one equipped/pending item. Plain fields + [Serializable] so it works
    /// with Unity's JsonUtility. Enums are stored as int. This is a pure data-transfer object; the
    /// domain's <c>EquipmentItem</c> remains the real model.
    /// </summary>
    [Serializable]
    public sealed class EquipmentItemDto
    {
        public string id;
        public int slot;
        public int rarity;
        public double accelerationBonus;
        public double topSpeedBonus;
    }

    /// <summary>
    /// Versioned Save Data V1. JsonUtility-friendly (public fields, [Serializable], arrays/lists,
    /// no dictionaries). Contains only persistent progression — never transient presentation state.
    /// </summary>
    [Serializable]
    public sealed class GameSaveDataV1
    {
        public int saveVersion;
        public long lastSavedUtcTicks;

        public long gold;
        public long wheels;

        public int campaignStageIndex;
        public bool autoBuildUnlocked;

        public int itemCreatorLevel;
        public int itemCreatorXp;

        /// <summary>Per-slot levels, indexed by (int)EquipmentSlotType (length 8).</summary>
        public int[] slotLevels;

        /// <summary>Equipped items (empty slots omitted).</summary>
        public List<EquipmentItemDto> equippedItems = new List<EquipmentItemDto>();

        /// <summary>Whether a generated item is awaiting EQUIP/DISCARD.</summary>
        public bool hasPendingItem;

        /// <summary>The pending item (valid only when <see cref="hasPendingItem"/> is true).</summary>
        public EquipmentItemDto pendingItem;
    }
}
