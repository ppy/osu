// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Osu.Judgements;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Osu.Objects
{
    public class SpinnerTick : OsuHitObject
    {
        public const int SCORE_PER_TICK = 10;

        public override Judgement CreateJudgement() => new OsuSpinnerTickJudgement();

        protected override HitWindows CreateHitWindows() => HitWindows.Empty;

        public class OsuSpinnerTickJudgement : OsuJudgement
        {
            public override bool AffectsCombo => false;

            protected override int NumericResultFor(HitResult result) => result == MaxResult ? SCORE_PER_TICK : 0;

            protected override double HealthIncreaseFor(HitResult result) => result == MaxResult ? 0.6 * base.HealthIncreaseFor(result) : 0;
        }
    }
}
