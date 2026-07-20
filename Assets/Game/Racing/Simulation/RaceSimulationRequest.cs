namespace IdleRacer.Racing.Simulation
{
    /// <summary>
    /// Immutable input for a single deterministic race simulation.
    /// <para>
    /// Units:
    /// <list type="bullet">
    /// <item><description><see cref="TrackDistance"/>: metres (m).</description></item>
    /// <item><description><see cref="FixedTimeStep"/>: seconds (s).</description></item>
    /// </list>
    /// </para>
    /// Value ranges are validated by <see cref="RaceSimulator.Simulate"/> so that a request
    /// remains a plain, allocation-free data carrier.
    /// </summary>
    public readonly struct RaceSimulationRequest
    {
        /// <summary>Stats for the player's car.</summary>
        public CarRaceStats PlayerStats { get; }

        /// <summary>Stats for the opponent's car.</summary>
        public CarRaceStats OpponentStats { get; }

        /// <summary>Length of the straight track, in metres (m). Must be &gt; 0.</summary>
        public double TrackDistance { get; }

        /// <summary>Fixed simulation timestep, in seconds (s). Must be &gt; 0.</summary>
        public double FixedTimeStep { get; }

        /// <summary>Creates a race simulation request.</summary>
        public RaceSimulationRequest(
            CarRaceStats playerStats,
            CarRaceStats opponentStats,
            double trackDistance,
            double fixedTimeStep)
        {
            PlayerStats = playerStats;
            OpponentStats = opponentStats;
            TrackDistance = trackDistance;
            FixedTimeStep = fixedTimeStep;
        }
    }
}
