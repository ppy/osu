// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Osu.Judgements;

namespace osu.Game.Rulesets.Osu.Objects
{
    public class SliderHeadCircle : HitCircle
    {
        /// <summary>
        /// Whether to treat this <see cref="SliderHeadCircle"/> as a normal <see cref="HitCircle"/> for judgement purposes.
        /// If <c>false</c>, this <see cref="SliderHeadCircle"/> will be judged as a <see cref="SliderTick"/> instead.
        /// </summary>
        public bool JudgeAsNormalHitCircle = true;

        public override Judgement CreateJudgement() => JudgeAsNormalHitCircle ? base.CreateJudgement() : new SliderTickJudgement();
    }
}
