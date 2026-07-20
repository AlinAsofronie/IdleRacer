using System;
using System.Collections.Generic;

namespace IdleRacer.Game.Equipment.Rarities
{
    /// <summary>
    /// A data-driven set of rarity probabilities that must sum to 100%. The exact same table is
    /// used both to display odds and to pick rarities, so displayed odds always match reality.
    /// </summary>
    public sealed class RarityTable
    {
        /// <summary>Tolerance (percentage points) allowed when validating the sum equals 100.</summary>
        public const double SumTolerance = 1e-6;

        private readonly RarityWeight[] _weights;

        public RarityTable(IEnumerable<RarityWeight> weights)
        {
            if (weights == null)
            {
                throw new ArgumentNullException(nameof(weights));
            }

            _weights = new List<RarityWeight>(weights).ToArray();
            if (_weights.Length == 0)
            {
                throw new ArgumentException("A rarity table must contain at least one weight.", nameof(weights));
            }

            double sum = TotalPercent();
            if (Math.Abs(sum - 100.0) > SumTolerance)
            {
                throw new ArgumentException($"Rarity probabilities must sum to 100 but summed to {sum}.", nameof(weights));
            }
        }

        /// <summary>The weights, in table order (for display).</summary>
        public IReadOnlyList<RarityWeight> Weights => _weights;

        /// <summary>Sum of all probabilities (percent). Should equal 100.</summary>
        public double TotalPercent()
        {
            double sum = 0.0;
            for (int i = 0; i < _weights.Length; i++)
            {
                sum += _weights[i].ProbabilityPercent;
            }
            return sum;
        }

        /// <summary>
        /// Selects a rarity for a roll in [0, 100). Uses cumulative probability so the outcome
        /// distribution exactly matches the configured (and displayed) percentages.
        /// </summary>
        public EquipmentRarity Pick(double rollPercent)
        {
            double cumulative = 0.0;
            for (int i = 0; i < _weights.Length; i++)
            {
                cumulative += _weights[i].ProbabilityPercent;
                if (rollPercent < cumulative)
                {
                    return _weights[i].Rarity;
                }
            }

            // Floating-point edge (roll ~100): fall back to the last configured rarity.
            return _weights[_weights.Length - 1].Rarity;
        }
    }
}
