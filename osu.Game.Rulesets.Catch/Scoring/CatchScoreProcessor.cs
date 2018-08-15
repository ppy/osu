// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Judgements;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Catch.Scoring
{
    public class CatchScoreProcessor : ScoreProcessor<CatchHitObject>
    {
        public CatchScoreProcessor(RulesetContainer<CatchHitObject> rulesetContainer)
            : base(rulesetContainer)
        {
        }

        private float hpDrainRate;

        protected override void ApplyBeatmap(Beatmap<CatchHitObject> beatmap)
        {
            base.ApplyBeatmap(beatmap);

            hpDrainRate = beatmap.BeatmapInfo.BaseDifficulty.DrainRate;
        }

        private const double harshness = 0.01;

        protected override void ApplyResult(JudgementResult result)
        {
            base.ApplyResult(result);

            if (result.Type == HitResult.Miss)
            {
                if (!result.Judgement.IsBonus)
                    Health.Value -= hpDrainRate * (harshness * 2);
                return;
            }

            if (result.Judgement is CatchJudgement catchJudgement)
                Health.Value += Math.Max(catchJudgement.HealthIncreaseFor(result) - hpDrainRate, 0) * harshness;
        }
    }
}
