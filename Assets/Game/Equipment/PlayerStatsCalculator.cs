using IdleRacer.Game.Equipment.Items;
using IdleRacer.Racing.Simulation;

namespace IdleRacer.Game.Equipment
{
    /// <summary>
    /// Combines the player's base stats with equipped-item bonuses to produce the final
    /// <see cref="CarRaceStats"/> used by the race simulator.
    /// <para>Final = Base + Sum(equipped flat bonuses). This is the simple v0.1C stat pipeline;
    /// the full multi-stage modifier pipeline arrives in a later milestone.</para>
    /// </summary>
    public static class PlayerStatsCalculator
    {
        /// <summary>
        /// Returns final race stats for the player.
        /// </summary>
        /// <param name="baseAcceleration">Base Acceleration in m/s^2 (before equipment).</param>
        /// <param name="baseTopSpeed">Base TopSpeed in m/s (before equipment).</param>
        /// <param name="loadout">Currently equipped items.</param>
        public static CarRaceStats CalculateRaceStats(double baseAcceleration, double baseTopSpeed, EquipmentLoadout loadout)
        {
            double acceleration = baseAcceleration + (loadout?.TotalAccelerationBonus() ?? 0.0);
            double topSpeed = baseTopSpeed + (loadout?.TotalTopSpeedBonus() ?? 0.0);
            return new CarRaceStats(acceleration, topSpeed);
        }
    }
}
