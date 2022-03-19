// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Osu.Judgements;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Mods.Objects
{
    public class StrictTrackingSliderTailCircle : SliderTailCircle
    {
        public StrictTrackingSliderTailCircle(Slider slider)
            : base(slider)
        {
        }

        public override Judgement CreateJudgement() => new OsuJudgement();
    }
}
