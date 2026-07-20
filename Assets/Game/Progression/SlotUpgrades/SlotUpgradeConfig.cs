using System;

namespace IdleRacer.Game.Progression.SlotUpgrades
{
    /// <summary>
    /// Data-driven progression curve shared by all equipment slots (v0.1D). One shared curve is
    /// used for prototype simplicity. All values are PROTOTYPE balance, not final.
    /// <para>Bonuses are cumulative for being at a level: level <see cref="StartingLevel"/> grants 0
    /// bonus; each level above adds <see cref="AccelerationBonusPerLevel"/> / <see cref="TopSpeedBonusPerLevel"/>.
    /// Upgrade cost grows geometrically from <see cref="BaseUpgradeCost"/>.</para>
    /// Units: Acceleration m/s^2, TopSpeed m/s, cost in Gold.
    /// </summary>
    public sealed class SlotUpgradeConfig
    {
        public int StartingLevel { get; }
        public double AccelerationBonusPerLevel { get; }
        public double TopSpeedBonusPerLevel { get; }
        public long BaseUpgradeCost { get; }
        public double CostGrowthFactor { get; }

        public SlotUpgradeConfig(
            int startingLevel,
            double accelerationBonusPerLevel,
            double topSpeedBonusPerLevel,
            long baseUpgradeCost,
            double costGrowthFactor)
        {
            if (startingLevel < 1) throw new ArgumentOutOfRangeException(nameof(startingLevel));
            if (baseUpgradeCost < 0) throw new ArgumentOutOfRangeException(nameof(baseUpgradeCost));
            if (costGrowthFactor <= 0) throw new ArgumentOutOfRangeException(nameof(costGrowthFactor));

            StartingLevel = startingLevel;
            AccelerationBonusPerLevel = accelerationBonusPerLevel;
            TopSpeedBonusPerLevel = topSpeedBonusPerLevel;
            BaseUpgradeCost = baseUpgradeCost;
            CostGrowthFactor = costGrowthFactor;
        }

        /// <summary>Cumulative Acceleration bonus (m/s^2) granted by a slot at <paramref name="level"/>.</summary>
        public double AccelerationBonusAtLevel(int level)
        {
            int steps = Math.Max(0, level - StartingLevel);
            return steps * AccelerationBonusPerLevel;
        }

        /// <summary>Cumulative TopSpeed bonus (m/s) granted by a slot at <paramref name="level"/>.</summary>
        public double TopSpeedBonusAtLevel(int level)
        {
            int steps = Math.Max(0, level - StartingLevel);
            return steps * TopSpeedBonusPerLevel;
        }

        /// <summary>Gold cost to upgrade a slot FROM <paramref name="currentLevel"/> to the next level.</summary>
        public long UpgradeCost(int currentLevel)
        {
            int steps = Math.Max(0, currentLevel - StartingLevel);
            double cost = BaseUpgradeCost * Math.Pow(CostGrowthFactor, steps);
            return (long)Math.Round(cost);
        }
    }
}
