// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Screens.Play.Settings;

namespace osu.Desktop.VisualTests.Tests
{
    internal class TestCaseReplaySettingsOverlay : TestCase
    {
        public override string Description => @"Settings visible in replay/auto";

        public override void Reset()
        {
            base.Reset();

            Add(new ReplaySettingsOverlay()
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
            });
        }
    }
}
