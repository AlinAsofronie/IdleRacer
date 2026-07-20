using System;
using IdleRacer.Game.Core.Economy;

namespace IdleRacer.Game.Progression.Campaign
{
    /// <summary>The outcome of applying a race result to the campaign.</summary>
    public readonly struct RaceResolution
    {
        public bool PlayerWon { get; }
        public bool Advanced { get; }
        public long GoldGranted { get; }
        public long WheelsGranted { get; }
        public bool AutoBuildJustUnlocked { get; }

        public RaceResolution(bool playerWon, bool advanced, long goldGranted, long wheelsGranted, bool autoBuildJustUnlocked)
        {
            PlayerWon = playerWon;
            Advanced = advanced;
            GoldGranted = goldGranted;
            WheelsGranted = wheelsGranted;
            AutoBuildJustUnlocked = autoBuildJustUnlocked;
        }
    }

    /// <summary>
    /// Owns campaign progression rules: which stage is current, granting rewards on a win,
    /// advancing on a win (retrying on a loss), and unlocking Auto Build after the final stage.
    /// UI/controllers call <see cref="ApplyRaceResult"/>; they never mutate state directly.
    /// </summary>
    public sealed class CampaignService
    {
        private readonly CampaignDefinition _definition;
        private readonly IEconomyService _economy;
        private readonly CampaignState _state;

        public CampaignService(CampaignDefinition definition, IEconomyService economy, CampaignState state = null)
        {
            _definition = definition ?? throw new ArgumentNullException(nameof(definition));
            _economy = economy ?? throw new ArgumentNullException(nameof(economy));
            _state = state ?? new CampaignState();
        }

        /// <summary>The stage currently being raced.</summary>
        public StageDefinition CurrentStage => _definition.Stages[_state.CurrentStageIndex];

        /// <summary>Zero-based index of the current stage.</summary>
        public int CurrentStageIndex => _state.CurrentStageIndex;

        /// <summary>True once Normal 1-10 has been beaten.</summary>
        public bool IsAutoBuildUnlocked => _state.AutoBuildUnlocked;

        /// <summary>True when the current stage is the final one in this campaign chapter.</summary>
        public bool IsOnFinalStage => _state.CurrentStageIndex >= _definition.LastStageIndex;

        /// <summary>
        /// Applies a race result. On a win: grants the stage's Gold/Wheel rewards, unlocks Auto
        /// Build if the final stage was beaten, and advances to the next stage (staying on the
        /// final stage so it remains repeatable). On a loss: no rewards, no advance (retry).
        /// </summary>
        public RaceResolution ApplyRaceResult(bool playerWon)
        {
            if (!playerWon)
            {
                return new RaceResolution(false, false, 0L, 0L, false);
            }

            StageDefinition stage = CurrentStage;
            _economy.Grant(CurrencyType.Gold, stage.GoldReward, TransactionReason.RaceReward);
            _economy.Grant(CurrencyType.Wheels, stage.WheelReward, TransactionReason.RaceReward);

            bool autoBuildJustUnlocked = false;
            bool advanced = false;

            if (IsOnFinalStage)
            {
                if (!_state.AutoBuildUnlocked)
                {
                    _state.AutoBuildUnlocked = true;
                    autoBuildJustUnlocked = true;
                }
                // Stay on the final stage so it remains a repeatable race.
            }
            else
            {
                _state.CurrentStageIndex++;
                advanced = true;
            }

            return new RaceResolution(true, advanced, stage.GoldReward, stage.WheelReward, autoBuildJustUnlocked);
        }
    }
}
