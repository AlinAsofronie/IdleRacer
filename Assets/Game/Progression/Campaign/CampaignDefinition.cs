using System;
using System.Collections.Generic;

namespace IdleRacer.Game.Progression.Campaign
{
    /// <summary>An ordered list of stages. For v0.1C this contains only Normal 1-1 .. 1-10.</summary>
    public sealed class CampaignDefinition
    {
        private readonly StageDefinition[] _stages;

        public CampaignDefinition(IEnumerable<StageDefinition> stages)
        {
            if (stages == null) throw new ArgumentNullException(nameof(stages));
            _stages = new List<StageDefinition>(stages).ToArray();
            if (_stages.Length == 0)
            {
                throw new ArgumentException("A campaign must contain at least one stage.", nameof(stages));
            }
        }

        public IReadOnlyList<StageDefinition> Stages => _stages;

        public int StageCount => _stages.Length;

        public int LastStageIndex => _stages.Length - 1;
    }
}
