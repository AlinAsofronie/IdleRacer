namespace IdleRacer.Game.Core.Economy
{
    /// <summary>
    /// Why a currency changed. Recorded on every transaction so balancing and debugging are
    /// possible. Extend as new sources/sinks are added.
    /// </summary>
    public enum TransactionReason
    {
        /// <summary>Initial/prototype starting balance.</summary>
        InitialGrant,

        /// <summary>Reward granted for winning a race/stage.</summary>
        RaceReward,

        /// <summary>Cost of generating an item in the Item Creator.</summary>
        ItemGenerationCost,

        /// <summary>Gold spent to permanently upgrade an equipment slot's level.</summary>
        EquipmentSlotUpgrade
    }
}
