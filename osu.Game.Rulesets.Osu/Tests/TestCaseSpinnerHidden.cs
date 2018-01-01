// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;

namespace osu.Game.Rulesets.Osu.Tests
{
    [Ignore("getting CI working")]
    public class TestCaseSpinnerHidden : TestCaseSpinner
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(Spinner),
            typeof(OsuModHidden),
            typeof(DrawableSpinner)
        };

        public TestCaseSpinnerHidden()
        {
            Mods.Add(new OsuModHidden());
        }
    }
}
