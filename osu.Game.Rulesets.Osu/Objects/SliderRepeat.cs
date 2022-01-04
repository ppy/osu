// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Osu.Judgements;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Osu.Objects
{
    public class SliderRepeat : SliderEndCircle
    {
        public SliderRepeat(Slider slider)
            : base(slider)
        {
        }

        public override Judgement CreateJudgement() => new SliderRepeatJudgement();

        public class SliderRepeatJudgement : OsuJudgement
        {
            public override HitResult MaxResult => HitResult.LargeTickHit;
        }
    }
}
