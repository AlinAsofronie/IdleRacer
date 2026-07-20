using IdleRacer.Game.Equipment.Rarities;

namespace IdleRacer.Game.Equipment.Items
{
    /// <summary>
    /// An immutable generated equipment item (v0.1C). Intentionally simple: identity, slot,
    /// rarity, and flat Acceleration/TopSpeed bonuses only. No affixes, tags, or refinement yet.
    /// Units: AccelerationBonus m/s^2, TopSpeedBonus m/s.
    /// </summary>
    public sealed class EquipmentItem
    {
        /// <summary>Unique identity of this generated item.</summary>
        public string Id { get; }

        /// <summary>Which slot this item occupies.</summary>
        public EquipmentSlotType Slot { get; }

        /// <summary>Rarity of the item.</summary>
        public EquipmentRarity Rarity { get; }

        /// <summary>Flat Acceleration bonus in m/s^2.</summary>
        public double AccelerationBonus { get; }

        /// <summary>Flat TopSpeed bonus in m/s.</summary>
        public double TopSpeedBonus { get; }

        public EquipmentItem(string id, EquipmentSlotType slot, EquipmentRarity rarity, double accelerationBonus, double topSpeedBonus)
        {
            Id = id;
            Slot = slot;
            Rarity = rarity;
            AccelerationBonus = accelerationBonus;
            TopSpeedBonus = topSpeedBonus;
        }
    }
}
