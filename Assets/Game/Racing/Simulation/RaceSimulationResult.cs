namespace IdleRacer.Racing.Simulation
{
    /// <summary>
    /// Immutable outcome of a race simulation. All times are in seconds (s).
    /// </summary>
    public readonly struct RaceSimulationResult
    {
        /// <summary>Who won the race (or <see cref="RaceWinner.Draw"/>).</summary>
        public RaceWinner Winner { get; }

        /// <summary>Time at which the player's car crossed the finish line, in seconds (s).</summary>
        public double PlayerFinishTime { get; }

        /// <summary>Time at which the opponent's car crossed the finish line, in seconds (s).</summary>
        public double OpponentFinishTime { get; }

        /// <summary>
        /// Absolute difference between the two finish times, in seconds (s).
        /// Always non-negative, regardless of who won.
        /// </summary>
        public double VictoryMarginSeconds { get; }

        /// <summary>Creates a race simulation result.</summary>
        public RaceSimulationResult(
            RaceWinner winner,
            double playerFinishTime,
            double opponentFinishTime,
            double victoryMarginSeconds)
        {
            Winner = winner;
            PlayerFinishTime = playerFinishTime;
            OpponentFinishTime = opponentFinishTime;
            VictoryMarginSeconds = victoryMarginSeconds;
        }
    }
}
