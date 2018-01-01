// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;

namespace osu.Game.Rulesets.Osu.Tests
{
    public class TestCaseSliderHidden : TestCaseSlider
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(Slider),
            typeof(HitCircle),
            typeof(SliderTick),
            typeof(RepeatPoint),
            typeof(OsuModHidden),
            typeof(DrawableSlider),
            typeof(DrawableHitCircle),
            typeof(DrawableSliderTick),
            typeof(DrawableRepeatPoint)
        };

        public TestCaseSliderHidden()
        {
            Mods.Add(new OsuModHidden());
        }
    }
}
