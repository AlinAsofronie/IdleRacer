using IdleRacer.Game.Equipment.Items;
using IdleRacer.Game.Progression.SlotUpgrades;
using IdleRacer.Racing.Simulation;

namespace IdleRacer.Game.Equipment
{
    /// <summary>
    /// The single authoritative path that combines the player's base stats, permanent equipment
    /// slot-level bonuses, and equipped-item bonuses into the final <see cref="CarRaceStats"/> used
    /// by the race simulator.
    /// <para>Final = Base + Sum(slot-level bonuses) + Sum(equipped item flat bonuses). This is the
    /// simple v0.1D stat pipeline; the full multi-stage modifier pipeline arrives later.</para>
    /// </summary>
    public static class PlayerStatsCalculator
    {
        /// <summary>Returns final race stats for the player.</summary>
        /// <param name="baseAcceleration">Base Acceleration in m/s^2 (before equipment/slots).</param>
        /// <param name="baseTopSpeed">Base TopSpeed in m/s (before equipment/slots).</param>
        /// <param name="loadout">Currently equipped items.</param>
        /// <param name="slotProgression">Permanent per-slot levels.</param>
        /// <param name="slotConfig">Slot progression curve providing per-level bonuses.</param>
        public static CarRaceStats CalculateRaceStats(
            double baseAcceleration,
            double baseTopSpeed,
            EquipmentLoadout loadout,
            EquipmentSlotProgression slotProgression,
            SlotUpgradeConfig slotConfig)
        {
            double acceleration = baseAcceleration;
            double topSpeed = baseTopSpeed;

            if (loadout != null)
            {
                acceleration += loadout.TotalAccelerationBonus();
                topSpeed += loadout.TotalTopSpeedBonus();
            }

            if (slotProgression != null && slotConfig != null)
            {
                acceleration += slotProgression.TotalAccelerationBonus(slotConfig);
                topSpeed += slotProgression.TotalTopSpeedBonus(slotConfig);
            }

            return new CarRaceStats(acceleration, topSpeed);
        }
    }
}
