// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Configuration;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Mania.Mods
{
    public class ManiaModPerfect : ModPerfect
    {
        [SettingSource("Full perfect", "Placeholder")]
        public BindableBool allPerfect { get; } = new BindableBool(false);

        protected override bool FailCondition(HealthProcessor healthProcessor, JudgementResult result)
        {
            if (!isRelevantResult(result.Judgement.MinResult) && !isRelevantResult(result.Judgement.MaxResult) && !isRelevantResult(result.Type))
                return false;

            if (allPerfect.Value && result.Judgement.MaxResult == HitResult.Perfect)
                return result.Type < HitResult.Perfect;

            // Mania allows imperfect "Great" hits without failing.
            if (result.Judgement.MaxResult == HitResult.Perfect)
                return result.Type < HitResult.Great;

            return result.Type != result.Judgement.MaxResult;
        }

        private bool isRelevantResult(HitResult result) => result.AffectsAccuracy() || result.AffectsCombo();
    }
}