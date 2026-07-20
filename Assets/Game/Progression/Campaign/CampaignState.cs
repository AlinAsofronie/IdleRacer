namespace IdleRacer.Game.Progression.Campaign
{
    /// <summary>
    /// Mutable, serialisable-friendly campaign progress. Mutated only through
    /// <see cref="CampaignService"/> so progression rules stay in one place.
    /// </summary>
    public sealed class CampaignState
    {
        /// <summary>Index into <see cref="CampaignDefinition.Stages"/> of the current stage.</summary>
        public int CurrentStageIndex { get; set; }

        /// <summary>True once the final Normal-1 stage (1-10) has been beaten (unlocks Auto Build).</summary>
        public bool AutoBuildUnlocked { get; set; }

        public CampaignState(int currentStageIndex = 0, bool autoBuildUnlocked = false)
        {
            CurrentStageIndex = currentStageIndex;
            AutoBuildUnlocked = autoBuildUnlocked;
        }
    }
}
