// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Taiko.Mods
{
    public class TaikoModHealthChallenge : ModHealthChallenge
    {
        protected override bool CheckThresholdCondition(HealthProcessor healthProcessor, JudgementResult result)
        {
            // Need to manually checked here since HasCompleted is updated after ApplyResultInternal
            bool allJudged = healthProcessor.JudgedHits == healthProcessor.MaxHits;

            return allJudged && base.CheckThresholdCondition(healthProcessor, result);
        }
    }
}
