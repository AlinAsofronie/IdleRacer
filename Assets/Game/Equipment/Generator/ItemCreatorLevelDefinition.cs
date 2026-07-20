using System;
using IdleRacer.Game.Equipment.Rarities;

namespace IdleRacer.Game.Equipment.Generator
{
    /// <summary>
    /// Configuration for one Item Creator level: which rarity table is active and how much XP is
    /// needed to reach the next level. Data-driven so levels/odds can be tuned without code changes.
    /// </summary>
    public sealed class ItemCreatorLevelDefinition
    {
        /// <summary>Level number (1-based).</summary>
        public int Level { get; }

        /// <summary>Rarity odds used while at this level (also what the UI displays).</summary>
        public RarityTable RarityTable { get; }

        /// <summary>
        /// XP (generated items) required to advance from this level to the next.
        /// Use 0 for the maximum level (no further progression).
        /// </summary>
        public int XpToNextLevel { get; }

        public ItemCreatorLevelDefinition(int level, RarityTable rarityTable, int xpToNextLevel)
        {
            if (level < 1) throw new ArgumentOutOfRangeException(nameof(level), level, "Level must be >= 1.");
            if (xpToNextLevel < 0) throw new ArgumentOutOfRangeException(nameof(xpToNextLevel), xpToNextLevel, "XpToNextLevel must be >= 0.");

            Level = level;
            RarityTable = rarityTable ?? throw new ArgumentNullException(nameof(rarityTable));
            XpToNextLevel = xpToNextLevel;
        }

        /// <summary>True when this is the maximum configured level.</summary>
        public bool IsMaxLevel => XpToNextLevel == 0;
    }
}
