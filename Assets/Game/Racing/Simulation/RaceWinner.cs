namespace IdleRacer.Racing.Simulation
{
    /// <summary>
    /// Outcome of a race simulation from the player's perspective.
    /// </summary>
    public enum RaceWinner
    {
        /// <summary>The player's car crossed the finish line first.</summary>
        Player,

        /// <summary>The opponent's car crossed the finish line first.</summary>
        Opponent,

        /// <summary>
        /// Both cars finished within <see cref="RaceSimulator.DrawToleranceSeconds"/> of each other.
        /// </summary>
        Draw
    }
}
