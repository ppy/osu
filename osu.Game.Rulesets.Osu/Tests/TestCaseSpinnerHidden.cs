﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Game.Rulesets.Osu.Mods;

namespace osu.Game.Rulesets.Osu.Tests
{
    [TestFixture]
    public class TestCaseSpinnerHidden : TestCaseSpinner
    {
        public override IReadOnlyList<Type> RequiredTypes => base.RequiredTypes.Concat(new[] { typeof(OsuModHidden) }).ToList();

        public TestCaseSpinnerHidden()
        {
            Mods.Add(new OsuModHidden());
        }
    }
}
