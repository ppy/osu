// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Game.Tests.Visual;
using osu.Game.Tournament.Screens.Showcase;

namespace osu.Game.Tournament.Tests
{
    public class TestCaseShowcase : OsuTestCase
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            Add(new ShowcaseScreen());
        }
    }
}
