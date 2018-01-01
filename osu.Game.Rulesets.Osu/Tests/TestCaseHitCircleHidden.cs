// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;

namespace osu.Game.Rulesets.Osu.Tests
{
    public class TestCaseHitCircleHidden : TestCaseHitCircle
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(HitCircle),
            typeof(OsuModHidden),
            typeof(DrawableHitCircle)
        };

        public TestCaseHitCircleHidden()
        {
            Mods.Add(new OsuModHidden());
        }
    }
}
