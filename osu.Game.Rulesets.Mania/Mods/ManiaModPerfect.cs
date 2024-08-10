// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Mania.Mods
{
    public class ManiaModPerfect : ModPerfect
    {
        public override FailState CheckFail(JudgementResult result)
        {
            if (!isRelevantResult(result.Judgement.MinResult) && !isRelevantResult(result.Judgement.MaxResult) && !isRelevantResult(result.Type))
                return FailState.Allow;

            // Mania allows imperfect "Great" hits without failing.
            if (result.Judgement.MaxResult == HitResult.Perfect)
                return result.Type >= HitResult.Great ? FailState.Allow : FailState.Force;

            return result.Type != result.Judgement.MaxResult ? FailState.Force : FailState.Allow;
        }

        private bool isRelevantResult(HitResult result) => result.AffectsAccuracy() || result.AffectsCombo();
    }
}
