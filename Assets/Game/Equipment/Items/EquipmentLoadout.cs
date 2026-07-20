using System;
using System.Collections.Generic;

namespace IdleRacer.Game.Equipment.Items
{
    /// <summary>
    /// The player's currently equipped items, one (optional) item per <see cref="EquipmentSlotType"/>.
    /// Equipping an item only ever affects its own slot. Serialisable-friendly: the equipped set
    /// can be enumerated via <see cref="GetEquipped"/>.
    /// </summary>
    public sealed class EquipmentLoadout
    {
        private readonly Dictionary<EquipmentSlotType, EquipmentItem> _equipped =
            new Dictionary<EquipmentSlotType, EquipmentItem>();

        /// <summary>Returns the item equipped in <paramref name="slot"/>, or <c>null</c> if empty.</summary>
        public EquipmentItem GetEquipped(EquipmentSlotType slot)
        {
            return _equipped.TryGetValue(slot, out EquipmentItem item) ? item : null;
        }

        /// <summary>
        /// Equips <paramref name="item"/> into its own slot, replacing only that slot. Returns the
        /// item previously in that slot (or <c>null</c>). Other slots are never affected.
        /// </summary>
        public EquipmentItem Equip(EquipmentItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            EquipmentItem previous = GetEquipped(item.Slot);
            _equipped[item.Slot] = item;
            return previous;
        }

        /// <summary>Total Acceleration bonus (m/s^2) from all equipped items.</summary>
        public double TotalAccelerationBonus()
        {
            double sum = 0.0;
            foreach (EquipmentItem item in _equipped.Values)
            {
                sum += item.AccelerationBonus;
            }
            return sum;
        }

        /// <summary>Total TopSpeed bonus (m/s) from all equipped items.</summary>
        public double TotalTopSpeedBonus()
        {
            double sum = 0.0;
            foreach (EquipmentItem item in _equipped.Values)
            {
                sum += item.TopSpeedBonus;
            }
            return sum;
        }
    }
}
