namespace IdleRacer.Game.Progression.Campaign
{
    /// <summary>
    /// Immutable, data-driven configuration for one campaign stage. The structure is designed to
    /// extend to Normal 2/3, Hard, and Hell later without code changes.
    /// Units: opponent Acceleration m/s^2, TopSpeed m/s, TrackDistance m.
    /// </summary>
    public sealed class StageDefinition
    {
        /// <summary>Stable identifier, e.g. "Normal-1-1".</summary>
        public string StageId { get; }

        /// <summary>Human-readable label, e.g. "NORMAL 1-1".</summary>
        public string DisplayName { get; }

        public double OpponentAcceleration { get; }
        public double OpponentTopSpeed { get; }
        public double TrackDistance { get; }

        public long GoldReward { get; }
        public long WheelReward { get; }

        public StageDefinition(
            string stageId,
            string displayName,
            double opponentAcceleration,
            double opponentTopSpeed,
            double trackDistance,
            long goldReward,
            long wheelReward)
        {
            StageId = stageId;
            DisplayName = displayName;
            OpponentAcceleration = opponentAcceleration;
            OpponentTopSpeed = opponentTopSpeed;
            TrackDistance = trackDistance;
            GoldReward = goldReward;
            WheelReward = wheelReward;
        }
    }
}
