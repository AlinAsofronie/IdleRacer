using System;
using IdleRacer.Game.Equipment.Items;
using IdleRacer.Game.Equipment.Rarities;

namespace IdleRacer.Game.Equipment.Generator
{
    /// <summary>
    /// Generates a single <see cref="EquipmentItem"/> from a rarity table and stat table using a
    /// supplied <see cref="Random"/>. Deterministic: the same seeded RNG and inputs always
    /// produce the same item. The generator never changes rarity odds — it only samples them.
    /// </summary>
    public sealed class ItemGenerator
    {
        /// <summary>Fractional variance applied around each rarity's base stat (±15%).</summary>
        public const double StatVariance = 0.15;

        private static readonly EquipmentSlotType[] AllSlots =
            (EquipmentSlotType[])Enum.GetValues(typeof(EquipmentSlotType));

        /// <summary>
        /// Generates one item. RNG draws happen in a fixed order (rarity, slot, accel variance,
        /// top variance, id) so results are reproducible for a given seed.
        /// </summary>
        public EquipmentItem Generate(RarityTable rarityTable, RarityStatTable statTable, Random random)
        {
            if (rarityTable == null) throw new ArgumentNullException(nameof(rarityTable));
            if (statTable == null) throw new ArgumentNullException(nameof(statTable));
            if (random == null) throw new ArgumentNullException(nameof(random));

            double rarityRoll = random.NextDouble() * 100.0;
            EquipmentRarity rarity = rarityTable.Pick(rarityRoll);

            EquipmentSlotType slot = AllSlots[random.Next(0, AllSlots.Length)];

            RarityStats baseStats = statTable.Get(rarity);
            double accelFactor = 1.0 + ((random.NextDouble() * 2.0) - 1.0) * StatVariance;
            double topFactor = 1.0 + ((random.NextDouble() * 2.0) - 1.0) * StatVariance;

            double accelerationBonus = Math.Round(baseStats.BaseAccelerationBonus * accelFactor, 2);
            double topSpeedBonus = Math.Round(baseStats.BaseTopSpeedBonus * topFactor, 2);

            string id = random.Next().ToString("x8") + random.Next().ToString("x8");

            return new EquipmentItem(id, slot, rarity, accelerationBonus, topSpeedBonus);
        }
    }
}
