// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Overlays;
using osu.Game.Users;

namespace osu.Game.Tests.Visual
{
    internal class TestCaseMedalOverlay : OsuTestCase
    {
        public override string Description => @"medal get!";

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
