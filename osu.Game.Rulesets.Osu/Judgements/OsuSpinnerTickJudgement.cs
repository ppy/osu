// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Osu.Judgements
{
    public class OsuSpinnerTickJudgement : OsuJudgement
    {
        internal bool HasBonusPoints;

        public override bool AffectsCombo => false;

        protected override int NumericResultFor(HitResult result) => 100 + (HasBonusPoints ? 1000 : 0);

        protected override double HealthIncreaseFor(HitResult result) => 0;
    }
}