// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Osu.Judgements;

namespace osu.Game.Rulesets.Osu.Objects
{
    public class SliderHeadCircle : HitCircle
    {
        /// <summary>
        /// If <see langword="false"/>, treat this <see cref="SliderHeadCircle"/> as a normal <see cref="HitCircle"/> for judgement purposes.
        /// If <see langword="true"/>, this <see cref="SliderHeadCircle"/> will be judged as a <see cref="SliderTick"/> instead.
        /// </summary>
        public bool ClassicSliderBehaviour;

        public override Judgement CreateJudgement() => ClassicSliderBehaviour ? new SliderTickJudgement() : base.CreateJudgement();
    }
}
