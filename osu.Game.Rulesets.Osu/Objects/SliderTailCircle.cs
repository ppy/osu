// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
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
    public class SliderTailCircle : SliderCircle
    {
        private readonly IBindable<SliderPath> pathBindable = new Bindable<SliderPath>();

        public SliderTailCircle(Slider slider)
        {
            pathBindable.BindTo(slider.PathBindable);
            pathBindable.BindValueChanged(_ => Position = slider.EndPosition);
        }

        public override Judgement CreateJudgement() => new OsuSliderTailJudgement();

        protected override HitWindows CreateHitWindows() => HitWindows.Empty;
    }
}
