namespace IdleRacer.Game.Equipment.Rarities
{
    /// <summary>A single rarity's drop probability (percent) within a <see cref="RarityTable"/>.</summary>
    public readonly struct RarityWeight
    {
        public EquipmentRarity Rarity { get; }

        /// <summary>Probability in percent (0..100). All weights in a table sum to 100.</summary>
        public double ProbabilityPercent { get; }

        public RarityWeight(EquipmentRarity rarity, double probabilityPercent)
        {
            Rarity = rarity;
            ProbabilityPercent = probabilityPercent;
        }
    }
}
