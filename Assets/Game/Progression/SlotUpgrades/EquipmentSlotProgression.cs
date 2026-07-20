using System;
using System.Collections.Generic;
using IdleRacer.Game.Equipment;

namespace IdleRacer.Game.Progression.SlotUpgrades
{
    /// <summary>
    /// Permanent per-slot levels. The level belongs to the SLOT, not to any equipped item, so
    /// replacing an item never changes a slot's level. Serialisable-friendly via
    /// <see cref="GetLevel"/> / <see cref="SetLevel"/>.
    /// </summary>
    public sealed class EquipmentSlotProgression
    {
        private static readonly EquipmentSlotType[] AllSlots =
            (EquipmentSlotType[])Enum.GetValues(typeof(EquipmentSlotType));

        private readonly Dictionary<EquipmentSlotType, int> _levels = new Dictionary<EquipmentSlotType, int>();
        private readonly int _startingLevel;

        public EquipmentSlotProgression(int startingLevel = 1)
        {
            _startingLevel = Math.Max(1, startingLevel);
            foreach (EquipmentSlotType slot in AllSlots)
            {
                _levels[slot] = _startingLevel;
            }
        }

        /// <summary>All slot types, in enum order.</summary>
        public static IReadOnlyList<EquipmentSlotType> Slots => AllSlots;

        public int GetLevel(EquipmentSlotType slot) => _levels[slot];

        public void SetLevel(EquipmentSlotType slot, int level)
        {
            _levels[slot] = Math.Max(_startingLevel, level);
        }

        /// <summary>Increases the slot's level by exactly one.</summary>
        public void IncrementLevel(EquipmentSlotType slot)
        {
            _levels[slot] = _levels[slot] + 1;
        }

        /// <summary>Total Acceleration bonus (m/s^2) from all slot levels, per <paramref name="config"/>.</summary>
        public double TotalAccelerationBonus(SlotUpgradeConfig config)
        {
            double sum = 0.0;
            foreach (EquipmentSlotType slot in AllSlots)
            {
                sum += config.AccelerationBonusAtLevel(_levels[slot]);
            }
            return sum;
        }

        /// <summary>Total TopSpeed bonus (m/s) from all slot levels, per <paramref name="config"/>.</summary>
        public double TotalTopSpeedBonus(SlotUpgradeConfig config)
        {
            double sum = 0.0;
            foreach (EquipmentSlotType slot in AllSlots)
            {
                sum += config.TopSpeedBonusAtLevel(_levels[slot]);
            }
            return sum;
        }
    }
}
