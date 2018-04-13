// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Game.Overlays;
using osu.Game.Overlays.MedalSplash;
using osu.Game.Users;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    public class TestCaseMedalOverlay : OsuTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(MedalOverlay),
            typeof(DrawableMedal),
        };

        public TestCaseMedalOverlay()
        {
            AddStep(@"display", () =>
            {
                LoadComponentAsync(new MedalOverlay(new Medal
                {
                    Name = @"Animations",
                    InternalName = @"all-intro-doubletime",
                    Description = @"More complex than you think.",
                }), Add);
            });
        }
    }
}
