// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Screens.Play.Settings;

namespace osu.Desktop.VisualTests.Tests
{
    internal class TestCaseSettingsDisplay : TestCase
    {
        public override string Description => @"Setting visible in replay/auto";

        public override void Reset()
        {
            base.Reset();

            Add(new SettingsDisplay()
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                AutoSizeAxes = Axes.Both,
            });
        }
    }
}
