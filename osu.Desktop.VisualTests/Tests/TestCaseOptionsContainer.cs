// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Overlays;
using osu.Game.Screens.Play.Options;

namespace osu.Desktop.VisualTests.Tests
{
    internal class TestCaseOptionsContainer : TestCase
    {
        public override string Description => @"Setting visible in replay/auto";

        public override void Reset()
        {
            base.Reset();

            Add(new OptionsDisplay()
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                AutoSizeAxes = Axes.Both,
            });
        }
    }
}
