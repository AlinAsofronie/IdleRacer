using System;
using System.Collections.Generic;

namespace IdleRacer.Game.Equipment.Generator
{
    /// <summary>Ordered set of <see cref="ItemCreatorLevelDefinition"/>s (level 1..N).</summary>
    public sealed class ItemCreatorConfig
    {
        private readonly ItemCreatorLevelDefinition[] _levels;

        public ItemCreatorConfig(IEnumerable<ItemCreatorLevelDefinition> levels)
        {
            if (levels == null) throw new ArgumentNullException(nameof(levels));
            _levels = new List<ItemCreatorLevelDefinition>(levels).ToArray();
            if (_levels.Length == 0)
            {
                throw new ArgumentException("At least one level must be configured.", nameof(levels));
            }
        }

        /// <summary>Highest configured level number.</summary>
        public int MaxLevel => _levels[_levels.Length - 1].Level;

        /// <summary>Returns the definition for <paramref name="level"/> (clamped to the configured range).</summary>
        public ItemCreatorLevelDefinition GetLevel(int level)
        {
            for (int i = 0; i < _levels.Length; i++)
            {
                if (_levels[i].Level == level)
                {
                    return _levels[i];
                }
            }

            // Clamp out-of-range requests to the nearest configured level.
            return level < _levels[0].Level ? _levels[0] : _levels[_levels.Length - 1];
        }
    }
}
