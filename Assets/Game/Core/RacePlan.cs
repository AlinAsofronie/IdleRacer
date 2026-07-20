using IdleRacer.Game.Progression.Campaign;
using IdleRacer.Racing.Simulation;

namespace IdleRacer.Game.Core
{
    /// <summary>
    /// A prepared race: the authoritative simulation result plus the stats used, so the visual
    /// layer can play it back. Rewards/advancement are applied separately via
    /// <see cref="GameController.ResolveRace"/> once playback finishes.
    /// </summary>
    public readonly struct RacePlan
    {
        public StageDefinition Stage { get; }
        public CarRaceStats PlayerStats { get; }
        public CarRaceStats OpponentStats { get; }
        public double TrackDistance { get; }
        public RaceSimulationResult Result { get; }

        public RacePlan(StageDefinition stage, CarRaceStats playerStats, CarRaceStats opponentStats, double trackDistance, RaceSimulationResult result)
        {
            Stage = stage;
            PlayerStats = playerStats;
            OpponentStats = opponentStats;
            TrackDistance = trackDistance;
            Result = result;
        }

        /// <summary>True when the player's car won (a Draw does not count as a win).</summary>
        public bool PlayerWon => Result.Winner == RaceWinner.Player;
    }
}
