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
        private readonly IBindable<int> pathVersion = new Bindable<int>();

        public SliderTailCircle(Slider slider)
        {
            pathVersion.BindTo(slider.Path.Version);
            pathVersion.BindValueChanged(_ => Position = slider.EndPosition);
        }

        protected override HitWindows CreateHitWindows() => HitWindows.Empty;

        public override Judgement CreateJudgement() => new SliderTailJudgement();

        public class SliderTailJudgement : OsuJudgement
        {
            public override HitResult MaxResult => HitResult.IgnoreHit;
        }
    }
}
