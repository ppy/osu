// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Configuration;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Judgements;

namespace osu.Game.Rulesets.Osu.Objects
{
    public class SliderTailCircle : SliderCircle
    {
        private readonly IBindable<SliderPath> pathBindable = new Bindable<SliderPath>();

        public SliderTailCircle(Slider slider)
        {
            pathBindable.BindTo(slider.PathBindable);
            pathBindable.BindValueChanged(_ => Position = slider.EndPosition);
        }

        public override Judgement CreateJudgement() => new OsuSliderTailJudgement();
    }
}
