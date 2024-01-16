// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Osu.Judgements;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Osu.Objects
{
    public class SpinnerTick : OsuHitObject
    {
        /// <summary>
        /// Duration of the <see cref="Spinner"/> containing this spinner tick.
        /// </summary>
        public double SpinnerDuration { get; set; }

        public override JudgementInfo CreateJudgement() => new OsuSpinnerTickJudgementInfo();

        protected override HitWindows CreateHitWindows() => HitWindows.Empty;

        public override double MaximumJudgementOffset => SpinnerDuration;

        public class OsuSpinnerTickJudgementInfo : OsuJudgementInfo
        {
            public override HitResult MaxResult => HitResult.SmallBonus;
        }
    }
}
