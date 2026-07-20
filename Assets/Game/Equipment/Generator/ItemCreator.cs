using System;
using IdleRacer.Game.Equipment.Rarities;

namespace IdleRacer.Game.Equipment.Generator
{
    /// <summary>
    /// Tracks the Item Creator's level and XP and exposes the currently active rarity table.
    /// Every generated item grants XP; reaching the configured threshold advances the level,
    /// which activates a different (better) rarity table. All values are configuration-driven.
    /// Serialisable-friendly via <see cref="Level"/> / <see cref="Xp"/>.
    /// </summary>
    public sealed class ItemCreator
    {
        private readonly ItemCreatorConfig _config;

        /// <summary>Raised when the level changes, with the new level.</summary>
        public event Action<int> LevelChanged;

        public ItemCreator(ItemCreatorConfig config, int startingLevel = 1, int startingXp = 0)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            Level = Math.Max(1, startingLevel);
            Xp = Math.Max(0, startingXp);
        }

        /// <summary>Current Item Creator level.</summary>
        public int Level { get; private set; }

        /// <summary>XP accumulated toward the next level.</summary>
        public int Xp { get; private set; }

        /// <summary>XP required to reach the next level (0 at max level).</summary>
        public int XpToNextLevel => _config.GetLevel(Level).XpToNextLevel;

        /// <summary>True when the creator is at its maximum configured level.</summary>
        public bool IsMaxLevel => _config.GetLevel(Level).IsMaxLevel;

        /// <summary>The rarity table currently in effect (used for generation and UI display).</summary>
        public RarityTable CurrentRarityTable => _config.GetLevel(Level).RarityTable;

        /// <summary>
        /// Grants XP for generating an item (default 1) and applies any resulting level-ups.
        /// </summary>
        public void AddGenerationXp(int amount = 1)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount), amount, "XP amount must be >= 0.");
            if (amount == 0) return;

            Xp += amount;

            // Apply as many level-ups as the accumulated XP allows.
            while (!IsMaxLevel)
            {
                int threshold = _config.GetLevel(Level).XpToNextLevel;
                if (Xp < threshold)
                {
                    break;
                }

                Xp -= threshold;
                Level++;
                LevelChanged?.Invoke(Level);
            }

            // At max level, XP no longer accumulates meaningfully.
            if (IsMaxLevel)
            {
                Xp = 0;
            }
        }
    }
}
