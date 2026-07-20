namespace IdleRacer.Game.Core.Economy
{
    /// <summary>Player currencies tracked by the economy (v0.1C).</summary>
    public enum CurrencyType
    {
        /// <summary>Earned from racing; accumulates this milestone (slot upgrades come later).</summary>
        Gold,

        /// <summary>Spent by the Item Creator: 1 Wheel per generated item by default.</summary>
        Wheels
    }
}
