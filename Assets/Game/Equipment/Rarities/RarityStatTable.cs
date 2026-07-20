using System;
using System.Collections.Generic;

namespace IdleRacer.Game.Equipment.Rarities
{
    /// <summary>Base flat stat bonus (m/s^2 acceleration, m/s top speed) granted per rarity.</summary>
    public readonly struct RarityStats
    {
        public double BaseAccelerationBonus { get; }
        public double BaseTopSpeedBonus { get; }

        public RarityStats(double baseAccelerationBonus, double baseTopSpeedBonus)
        {
            BaseAccelerationBonus = baseAccelerationBonus;
            BaseTopSpeedBonus = baseTopSpeedBonus;
        }
    }

    /// <summary>
    /// Data-driven mapping of rarity to its base stat bonuses. The item generator applies a
    /// small deterministic variance around these bases.
    /// </summary>
    public sealed class RarityStatTable
    {
        private readonly Dictionary<EquipmentRarity, RarityStats> _stats;

        public RarityStatTable(IDictionary<EquipmentRarity, RarityStats> stats)
        {
            if (stats == null)
            {
                throw new ArgumentNullException(nameof(stats));
            }
            _stats = new Dictionary<EquipmentRarity, RarityStats>(stats);
        }

        public RarityStats Get(EquipmentRarity rarity)
        {
            if (!_stats.TryGetValue(rarity, out RarityStats value))
            {
                throw new ArgumentOutOfRangeException(nameof(rarity), rarity, "No base stats configured for rarity.");
            }
            return value;
        }
    }
}
