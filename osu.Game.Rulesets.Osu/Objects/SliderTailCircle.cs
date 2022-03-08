// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Judgements;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Osu.Objects
{
    /// <summary>
    /// Note that this should not be used for timing correctness.
    /// See <see cref="SliderEventType.LegacyLastTick"/> usage in <see cref="Slider"/> for more information.
    /// </summary>
    public class SliderTailCircle : SliderEndCircle
    {
        public bool JudgeAsSliderTick = false;

        public SliderTailCircle(Slider slider)
            : base(slider)
        {
        }

        public override Judgement CreateJudgement() => JudgeAsSliderTick ? (OsuJudgement)new SliderTickJudgement() : new SliderTailJudgement();

        public class SliderTailJudgement : OsuJudgement
        {
            public override HitResult MaxResult => HitResult.SmallTickHit;
        }
    }
}
